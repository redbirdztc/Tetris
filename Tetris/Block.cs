namespace Tetris
{
    public class Block
    {
        // x,y
        protected int[][] points;

        // x,y
        protected int[] rotationPoint;

        public Block()
        {
        }

        public Block(int[][] points, int[] rotationPoint)
        {
            this.rotationPoint = rotationPoint;
            this.points = points;
        }

        public void Rotate()
        {
            foreach (var point in points)
            {
                var xOffset = point[0] - rotationPoint[0];
                var yOffset = point[1] - rotationPoint[1];

                if ((xOffset < 0 && yOffset < 0) || (xOffset > 0 && yOffset > 0))
                {
                    yOffset = -yOffset;
                }
                else
                {
                    xOffset = -xOffset;
                }

                point[0] = rotationPoint[0] + yOffset;
                point[1] = rotationPoint[1] + xOffset;
            }
        }

        public void ContraRotate()
        {
            foreach (var point in points)
            {
                var xOffset = point[0] - rotationPoint[0];
                var yOffset = point[1] - rotationPoint[1];

                if ((xOffset < 0 && yOffset < 0) || (xOffset > 0 && yOffset > 0))
                {
                    xOffset = -xOffset;
                }
                else
                {
                    yOffset = -yOffset;
                }

                point[0] = rotationPoint[0] + yOffset;
                point[1] = rotationPoint[1] + xOffset;
            }
        }

        public void Fall()
        {
            foreach (var point in points)
            {
                point[1]++;
            }
        }

        public void Rise()
        {
            foreach (var point in points)
            {
                point[1]--;
            }
        }
    }

    public class BlockI : Block
    {
        public BlockI(int[] rotationPoint)
        {
            this.rotationPoint = rotationPoint;
            points = new int[4][];
            points[0] = new[] { rotationPoint[0], rotationPoint[1] };
            points[1] = new[] { points[0][0], points[0][1] + 1 };
            points[2] = new[] { points[0][0], points[0][1] - 1 };
            points[3] = new[] { points[0][0], points[0][1] + 2 };
        }
    }
}