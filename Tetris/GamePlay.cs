using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

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

        public GamePlay(Tetris form, int rows, int cols)
        {
            _form = form;
            _playArea = new bool[rows + 2][];
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
                Render();
                if (operationQueue.Count > 0)
                {
                    var shouldSetDown = HandleUserOperation();

                    Render();
                    operationQueue.Clear();

                    if (shouldSetDown)
                    {
                        goto afterQuickFall;
                    }
                }
                else
                {
                    // have a nap
                    Thread.Sleep(TimeSpan.FromMilliseconds(5));
                }

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

                if (!shouldBeSetDown && !FallingBlockHasConflictsWithBottom())
                {
                    nextRenderTime = nextRenderTime.Add(timeInterval);
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

                    goto startGame;
                }

                SetFallingBlockDown();
                EliminateCompletedRow();
                _fallingBlock = NewBlock();
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
                        _fallingBlock.MoveLeft();
                        if (_fallingBlock.GetPoints().Any(point => point.Any(col => col < 0)) ||
                            FallingBlockHasConflictsWithBottom())
                        {
                            _fallingBlock.MoveRight();
                        }

                        Console.WriteLine(@"left move");
                        break;
                    case "d":
                        _fallingBlock.MoveRight();
                        if (_fallingBlock.GetPoints()
                                .Any(point => point[1] >= _playArea[0].Length) ||
                            FallingBlockHasConflictsWithBottom())
                        {
                            _fallingBlock.MoveLeft();
                        }

                        Console.WriteLine(@"right move");
                        break;
                    case "w":
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
                    case "s": break;
                    case "j":
                        for (; !FallingBlockHasConflictsWithBottom();)
                        {
                            _fallingBlock.Fall();
                            foreach (var point in _fallingBlock.GetPoints())
                            {
                                if (point[0] >= _playArea.Length)
                                {
                                    _fallingBlock.Rise();
                                    return true;
                                }
                            }
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

                for (var j = 0; j < _playArea[i].Length; j++)
                {
                    _playArea[i + rowFallArray[i]] = _playArea[i];
                }
            }
        }

        private bool IsFallingBlockOverTheTop()
        {
            return _fallingBlock.GetPoints().Any(point => point[0] < 2);
        }

        private void SetFallingBlockDown()
        {
            foreach (var point in _fallingBlock.GetPoints())
            {
                _playArea[point[0]][point[1]] = true;
            }
        }

        // render 
        private void Render()
        {
            var status = _status;
            _status = "Rendering";
            var cellContainer = _form.GetCellContainer();
            _form.SuspendLayout();
            for (var i = 2; i < _playArea.Length; i++)
            {
                for (var j = _playArea[i].Length - 1; j >= 0; j--)
                {
                    cellContainer[i - 2][j].BackColor =
                        _playArea[i][j] ? Color.Gray : Color.Black;
                }
            }

            foreach (var point in _fallingBlock.GetPoints())
            {
                if (point[0] - 2 < 0)
                {
                    continue;
                }

                cellContainer[point[0] - 2][point[1]].BackColor = Color.Gray;
            }

            _form.ResumeLayout();
            _status = status;
        }

        // block conflict check
        private bool FallingBlockHasConflictsWithBottom()
        {
            var fallingPoints = _fallingBlock.GetPoints();
            return fallingPoints.Any(fallingPoint =>
                fallingPoint[0] >= _playArea.Length || _playArea[fallingPoint[0]][fallingPoint[1]]);
        }

        private Block NewBlock()
        {
            var r = new Random().Next(6);
            // return new BlockZ(new[] { 2, 5 });
            switch (r)
            {
                case 0:
                    return new BlockI(new[] { 2, 5 });
                case 1:
                    return new BlockO(new[] { 2, 5 });
                case 2:
                    return new BlockZ(new[] { 2, 5 });
                case 3:
                    return new BlockS(new[] { 2, 5 });
                case 4:
                    return new BlockJ(new[] { 2, 5 });
                case 5:
                    return new BlockL(new[] { 2, 5 });
                default:
                    return NewBlock();
            }
        }
    }
}