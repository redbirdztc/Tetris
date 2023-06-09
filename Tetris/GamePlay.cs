using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Serilog.Core;

namespace Tetris
{
    public class GamePlay
    {
        private readonly Tetris _form;

        // game play area
        // 2 rows higher than cell container
        // row-col, top-left to bottom-right
        private bool[][] _playArea;
        // input listening

        // current block
        private Block _fallingBlock;

        // finite state machine
        // Ready, Falling, Rendering, End
        private string _status;

        private List<string> operationQueue;

        public Logger Log { get; set; }

        public GamePlay(Logger log, Tetris form, int rows, int cols)
        {
            _form = form;
            _playArea = new bool[rows + 2][];
            Log = log;
            for (var i = 0; i < _playArea.Length; i++)
            {
                _playArea[i] = new bool[cols];
            }

            _status = "Ready";
            operationQueue = new List<string>();

            _form.KeyPress += HandleKeyPress;
        }

        // time ticker
        public void Start()
        {
            startGame:
            _status = "Falling";

            _fallingBlock = NewBlock();
            var now = DateTime.Now;
            var timeInterval = TimeSpan.FromMilliseconds(500);
            var nextRenderTime = now.Add(timeInterval);

            for (;;)
            {
                if (operationQueue.Count > 0)
                {
                    var shouldSetDown = HandleUserOperation();

                    operationQueue.Clear();

                    if (shouldSetDown)
                    {
                        goto afterQuickFall;
                    }

                    Render();
                    continue;
                }

                // have a nap
                Thread.Sleep(TimeSpan.FromMilliseconds(5));

                if (DateTime.Now.CompareTo(nextRenderTime) < 0) continue;

                _fallingBlock.Fall();
                // if falling block has conflicts with bottom blocks or no blocks downside when falling block is at bottom,
                // set it down and generate a new block
                var shouldBeSetDown = false;
                foreach (var point in _fallingBlock.GetPoints())
                {
                    if (point[0] >= _playArea.Length)
                    {
                        shouldBeSetDown = true;
                    }
                }

                shouldBeSetDown |= FallingBlockHasConflictsWithBottom();

                if (!shouldBeSetDown)
                {
                    nextRenderTime = nextRenderTime.Add(timeInterval);
                    Render();
                    continue;
                }

                _fallingBlock.Rise();

                afterQuickFall:
                // if top of the block is over the top side, the game's end
                if (IsFallingBlockOverTheTop())
                {
                    var col = _playArea[0].Length;
                    _playArea = new bool[_playArea.Length][];
                    for (var i = 0; i < _playArea.Length; i++)
                    {
                        _playArea[i] = new bool[col];
                    }

                    Render();
                    goto startGame;
                }

                SetFallingBlockDown();
                EliminateCompletedRow();
                _fallingBlock = NewBlock();
                Render();
            }
        }

        // return bool when falling block should be set down
        private bool HandleUserOperation()
        {
            // handle the operations
            foreach (var operation in operationQueue)
            {
                switch (operation)
                {
                    case "a":
                    case "A":
                        _fallingBlock.MoveLeft();
                        if (_fallingBlock.GetPoints().Any(point => point.Any(col => col < 0)) ||
                            FallingBlockHasConflictsWithOtherBlocks())
                        {
                            _fallingBlock.MoveRight();
                        }

                        break;
                    case "d":
                    case "D":
                        _fallingBlock.MoveRight();
                        if (_fallingBlock.GetPoints().Any(point => point[1] >= _playArea[0].Length) ||
                            FallingBlockHasConflictsWithOtherBlocks())
                        {
                            _fallingBlock.MoveLeft();
                        }

                        break;
                    case "w":
                        case "W":
                        _fallingBlock.RecordState();
                        _fallingBlock.BlockGameRotate();
                        if (_fallingBlock.GetPoints()
                            .Any(point => point[1] >= _playArea[0].Length))
                        {
                            _fallingBlock.MoveLeft();
                        }

                        if (_fallingBlock.GetPoints().Any(point => point[1] < 0))
                        {
                            _fallingBlock.MoveRight();
                        }

                        if (FallingBlockHasConflictsWithBottom())
                        {
                            _fallingBlock.Recover();
                        }

                        break;
                    case "s":
                        case "S":
                        _fallingBlock.MoveDown();
                        if (FallingBlockHasConflictsWithBottom())
                        {
                            _fallingBlock.Rise();
                        }

                        break;
                    case "j":
                    case "J":
                        for (; !FallingBlockHasConflictsWithBottom(); _fallingBlock.Fall())
                        {
                            if (!_fallingBlock.GetPoints().Any(point => point[0] >= _playArea.Length))
                            {
                                continue;
                            }

                            break;
                        }

                        _fallingBlock.Rise();
                        return true;
                }
            }

            return false;
        }

        private void HandleKeyPress(object sender, KeyPressEventArgs e)
        {
            var input = e.KeyChar.ToString();
            if (new[] { "w", "s", "a", "d", "j" }.Contains(input))
            {
                Log.Debug("operation {o} received", e.KeyChar);
                operationQueue.Add(input);
            }

            e.Handled = true;
        }

        // logic of eliminating row
        private void EliminateCompletedRow()
        {
            var rowFallArray = new int[_playArea.Length];

            // calculate num of rows for each row's falling
            for (var i = _playArea.Length - 1; i >= 0; i--)
            {
                foreach (var b in _playArea[i])
                {
                    if (!b)
                    {
                        goto endOfLoop;
                    }
                }

                Log.Debug("row {row} eliminated", i);
                // upper rows falls +1
                for (var j = i - 1; j > 1; j--)
                {
                    rowFallArray[j]++;
                }

                endOfLoop: ;
            }

            // set all rows final position
            for (var i = _playArea.Length - 2; i >= 0; i--)
            {
                if (rowFallArray[i] == 0)
                {
                    continue;
                }

                _playArea[i + rowFallArray[i]] = _playArea[i];
                _playArea[i] = new bool[_playArea[0].Length];
            }
        }

        private bool IsFallingBlockOverTheTop()
        {
            return _fallingBlock.GetPoints().Any(point => point[0] < 2);
        }

        private void SetFallingBlockDown()
        {
            var points = _fallingBlock.GetPoints();
            Log.Debug("set falling block down: {fallingBlock}", points);
            foreach (var point in points)
            {
                _playArea[point[0]][point[1]] = true;
            }
        }

        // render 
        private void Render()
        {
            // Log.Debug("render play area : {playArea}",_playArea);
            var status = _status;
            _status = "Rendering";
            var cellContainer = _form.GetCellContainer();
            _form.SuspendLayout();
            for (var i = 2; i < _playArea.Length; i++)
            {
                for (var j = _playArea[i].Length - 1; j >= 0; j--)
                {
                    if (!cellContainer[i - 2][j].BackColor.Equals(_playArea[i][j] ? Color.Gray : Color.Black))
                    {
                        cellContainer[i - 2][j].BackColor = _playArea[i][j] ? Color.Gray : Color.Black;
                    }
                }
            }

            foreach (var point in _fallingBlock.GetPoints())
            {
                if (point[0] - 2 < 0)
                {
                    continue;
                }

                if (!cellContainer[point[0] - 2][point[1]].BackColor.Equals(Color.Gray))
                {
                    cellContainer[point[0] - 2][point[1]].BackColor = Color.Gray;
                }
            }

            _form.ResumeLayout();
            _status = status;
        }

        // block conflict check
        private bool FallingBlockHasConflictsWithOtherBlocks()
        {
            return _fallingBlock.GetPoints().Any(point => _playArea[point[0]][point[1]]);
        }

        private bool FallingBlockHasConflictsWithBottom()
        {
            var fallingPoints = _fallingBlock.GetPoints();
            var hasConflict = fallingPoints.Any(fallingPoint => fallingPoint[0] >= _playArea.Length) ||
                              FallingBlockHasConflictsWithOtherBlocks();

            if (hasConflict)
            {
                Log.Debug("reach bottom. points: {point0}, {point1}, {point2}, {point3}",
                    fallingPoints[0], fallingPoints[1], fallingPoints[2], fallingPoints[3]);
            }

            return hasConflict;
        }

        private Block NewBlock()
        {
            var r = new Random().Next(7);

            Block block;
            switch (r)
            {
                case 0:
                    block = new BlockI(new[] { 2, 5 });
                    break;
                case 1:
                    block = new BlockO(new[] { 2, 5 });
                    break;
                case 2:
                    block = new BlockZ(new[] { 2, 5 });
                    break;
                case 3:
                    block = new BlockS(new[] { 2, 5 });
                    break;
                case 4:
                    block = new BlockJ(new[] { 2, 5 });
                    break;
                case 5:
                    block = new BlockL(new[] { 2, 5 });
                    break;
                case 6:
                    block = new BlockT(new[] { 2, 5 });
                    break;
                default:
                    return NewBlock();
            }

            Log.Debug("create block");
            return block;
        }
    }
}