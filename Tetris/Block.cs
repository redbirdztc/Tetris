using System.Collections.Generic;

namespace Tetris
{
    public class Block
    {
        // row-col 
        protected int[][] Points;

        private int[][] RecordedPoints;

        public IEnumerable<int[]> GetPoints()
        {
            return Points;
        }

        // x,y
        protected int[] RotationPoint;

        protected Block()
        {
        }

        public Block(int[][] points, int[] rotationPoint)
        {
            this.RotationPoint = rotationPoint;
            this.Points = points;
        }

        public void Rotate()
        {
            foreach (var point in Points)
            {
                var xOffset = point[1] - RotationPoint[1];
                var yOffset = point[0] - RotationPoint[0];

                point[1] = RotationPoint[1] - yOffset;
                point[0] = RotationPoint[0] + xOffset;
            }
        }

        public void ContraRotate()
        {
            foreach (var point in Points)
            {
                var xOffset = point[1] - RotationPoint[1];
                var yOffset = point[0] - RotationPoint[0];

                point[1] = RotationPoint[1] + yOffset;
                point[0] = RotationPoint[0] - xOffset;
            }
        }

        public virtual void BlockGameRotate()
        {
            Rotate();
        }

        public void Fall()
        {
            foreach (var point in Points)
            {
                point[0]++;
            }

            RotationPoint[0]++;
        }

        public void Rise()
        {
            foreach (var point in Points)
            {
                point[0]--;
            }

            RotationPoint[0]--;
        }

        public void MoveLeft()
        {
            foreach (var point in Points)
            {
                point[1]--;
            }

            RotationPoint[1]--;
        }

        public void MoveRight()
        {
            foreach (var point in Points)
            {
                point[1]++;
            }

            RotationPoint[1]++;
        }

        public void MoveDown()
        {
            foreach (var point in Points)
            {
                point[0]++;
            }

            RotationPoint[0]++;
        }

        public void RecordState()
        {
            if (RecordedPoints == null)
            {
                RecordedPoints = new int[Points.Length][];
                for (var i = 0; i < RecordedPoints.Length; i++)
                {
                    RecordedPoints[i] = new int[Points[0].Length];
                }
            }

            for (var i = 0; i < Points.Length; i++)
            {
                for (var j = 0; j < Points[i].Length; j++)
                {
                    RecordedPoints[i][j] = Points[i][j];
                }
            }
        }

        public void Recover()
        {
            for (var i = 0; i < Points.Length; i++)
            {
                for (var j = 0; j < Points[i].Length; j++)
                {
                    Points[i][j] = RecordedPoints[i][j];
                }
            }
        }
    }

    public class BlockI : Block
    {
        private int _rotateStatus;

        public BlockI(int[] rotationPoint)
        {
            RotationPoint = rotationPoint;
            Points = new int[4][];
            Points[0] = new[] { rotationPoint[0], rotationPoint[1] };
            Points[1] = new[] { Points[0][0] + 1, Points[0][1] };
            Points[2] = new[] { Points[0][0] - 1, Points[0][1] };
            Points[3] = new[] { Points[0][0] + 2, Points[0][1] };
        }

        public override void BlockGameRotate()
        {
            if (_rotateStatus == 0)
            {
                base.ContraRotate();
                _rotateStatus++;
            }
            else
            {
                base.Rotate();
                _rotateStatus--;
            }
        }
    }

    public class BlockO : Block
    {
        public BlockO(int[] rotationPoint)
        {
            RotationPoint = rotationPoint;
            Points = new int[4][];
            Points[0] = new[] { rotationPoint[0], rotationPoint[1] };
            Points[1] = new[] { Points[0][0] - 1, Points[0][1] };
            Points[2] = new[] { Points[0][0] - 1, Points[0][1] - 1 };
            Points[3] = new[] { Points[0][0], Points[0][1] - 1 };
        }

        public override void BlockGameRotate()
        {
        }
    }

    public class BlockS : Block
    {
        private int _rotateStatus;

        public BlockS(int[] rotationPoint)
        {
            RotationPoint = rotationPoint;
            Points = new int[4][];
            Points[0] = new[] { rotationPoint[0], rotationPoint[1] };
            Points[1] = new[] { Points[0][0] - 1, Points[0][1] };
            Points[2] = new[] { Points[0][0] - 1, Points[0][1] + 1 };
            Points[3] = new[] { Points[0][0], Points[0][1] - 1 };
        }

        public override void BlockGameRotate()
        {
            if (_rotateStatus == 0)
            {
                base.ContraRotate();
                _rotateStatus++;
            }
            else
            {
                base.Rotate();
                _rotateStatus--;
            }
        }
    }

    public class BlockZ : Block
    {
        private int _rotateStatus;

        public BlockZ(int[] rotationPoint)
        {
            RotationPoint = rotationPoint;
            Points = new int[4][];
            Points[0] = new[] { rotationPoint[0], rotationPoint[1] };
            Points[1] = new[] { Points[0][0], Points[0][1] + 1 };
            Points[2] = new[] { Points[0][0] - 1, Points[0][1] };
            Points[3] = new[] { Points[0][0] - 1, Points[0][1] - 1 };
        }

        public override void BlockGameRotate()
        {
            if (_rotateStatus == 0)
            {
                base.ContraRotate();
                _rotateStatus++;
            }
            else
            {
                base.Rotate();
                _rotateStatus--;
            }
        }
    }

    public class BlockJ : Block
    {
        public BlockJ(int[] rotationPoint)
        {
            RotationPoint = rotationPoint;
            Points = new int[4][];
            Points[0] = new[] { rotationPoint[0], rotationPoint[1] };
            Points[1] = new[] { Points[0][0] - 1, Points[0][1] };
            Points[2] = new[] { Points[0][0] - 2, Points[0][1] };
            Points[3] = new[] { Points[0][0], Points[0][1] - 1 };
        }

        public override void BlockGameRotate()
        {
            base.ContraRotate();
        }
    }

    public class BlockL : Block
    {
        public BlockL(int[] rotationPoint)
        {
            RotationPoint = rotationPoint;
            Points = new int[4][];
            Points[0] = new[] { rotationPoint[0], rotationPoint[1] };
            Points[1] = new[] { Points[0][0] - 1, Points[0][1] };
            Points[2] = new[] { Points[0][0] - 2, Points[0][1] };
            Points[3] = new[] { Points[0][0], Points[0][1] + 1 };
        }
    }

    public class BlockT : Block
    {
        public BlockT(int[] rotationPoint)
        {
            RotationPoint = rotationPoint;
            Points = new int[4][];
            Points[0] = new[] { rotationPoint[0], rotationPoint[1] };
            Points[1] = new[] { Points[0][0] - 1, Points[0][1] };
            Points[2] = new[] { Points[0][0], Points[0][1] - 1 };
            Points[3] = new[] { Points[0][0], Points[0][1] + 1 };
        }
    }
}