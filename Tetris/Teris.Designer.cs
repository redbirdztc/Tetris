namespace Tetris
{
    partial class Teris
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Keep cell containers will light when blocks reach and darken when blocks leave.
        /// </summary>
        private System.Windows.Forms.Panel[][] CellContainer;
        private void InitGameBlockContainer()
        {
            this.SuspendLayout();
            var cellsRow=new System.Windows.Forms.Panel[20][];
            this.CellContainer = cellsRow;

            var row = 0;
            
            for (var i = 0; i < cellsRow.Length; i++)
            {
                var col = 0;
                var cellsCol= new System.Windows.Forms.Panel[10];
                this.CellContainer[i] = cellsCol;
                
                
                for (var j = 0; j < cellsCol.Length; j++)
                {
                    cellsCol[j]=new System.Windows.Forms.Panel();
                     cellsCol[j].BackColor = System.Drawing.Color.Black;
                     cellsCol[j].Location = new System.Drawing.Point(this.GameArea.Location.X+4+col*20,this.GameArea.Location.Y+ 4+row*20);
                     cellsCol[j].Name = "GameArea";
                     cellsCol[j].Size = new System.Drawing.Size(16, 16);
                     this.Controls.Add(cellsCol[j]);
                     cellsCol[j].BringToFront();
                     col++;
                }

                row++;
            }
            this.ResumeLayout();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.GameArea = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // GameArea
            // 
            this.GameArea.BackColor = System.Drawing.Color.Black;
            this.GameArea.Location = new System.Drawing.Point(12, 12);
            this.GameArea.Name = "GameArea";
            this.GameArea.Size = new System.Drawing.Size(204, 404);
            this.GameArea.TabIndex = 0;
            // 
            // Teris
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 450);
            this.Controls.Add(this.GameArea);
            this.Name = "Teris";
            this.Text = "Tetris";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel GameArea;

        #endregion
    }
}