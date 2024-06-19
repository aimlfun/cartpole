namespace Cart
{
    partial class FormCartPoleGym
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pictureBox1 = new PictureBox();
            buttonAIPlay = new Button();
            buttonLeft = new Button();
            buttonRight = new Button();
            timerMove = new System.Windows.Forms.Timer(components);
            labelState = new Label();
            label1 = new Label();
            toolTip1 = new ToolTip(components);
            buttonYouPlay = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(2, 1);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(600, 400);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // buttonAIPlay
            // 
            buttonAIPlay.Location = new Point(12, 408);
            buttonAIPlay.Name = "buttonAIPlay";
            buttonAIPlay.Size = new Size(75, 23);
            buttonAIPlay.TabIndex = 1;
            buttonAIPlay.Text = "AI Play";
            toolTip1.SetToolTip(buttonAIPlay, "Click this, and weep. This is how a pro does it.");
            buttonAIPlay.UseVisualStyleBackColor = true;
            buttonAIPlay.Click += ButtonAIPlays_Click;
            // 
            // buttonLeft
            // 
            buttonLeft.Location = new Point(316, 408);
            buttonLeft.Name = "buttonLeft";
            buttonLeft.Size = new Size(75, 23);
            buttonLeft.TabIndex = 2;
            buttonLeft.Text = "Left";
            toolTip1.SetToolTip(buttonLeft, "Steer left (arrow key is easier).");
            buttonLeft.UseVisualStyleBackColor = true;
            buttonLeft.Click += ButtonLeft_Click;
            // 
            // buttonRight
            // 
            buttonRight.Location = new Point(393, 408);
            buttonRight.Name = "buttonRight";
            buttonRight.Size = new Size(75, 23);
            buttonRight.TabIndex = 3;
            buttonRight.Text = "Right";
            toolTip1.SetToolTip(buttonRight, "Steer right (arrow key is easier).");
            buttonRight.UseVisualStyleBackColor = true;
            buttonRight.Click += ButtonRight_Click;
            // 
            // timerMove
            // 
            timerMove.Interval = 200;
            timerMove.Tick += TimerMove_Tick;
            // 
            // labelState
            // 
            labelState.AutoSize = true;
            labelState.Location = new Point(172, 412);
            labelState.Name = "labelState";
            labelState.Size = new Size(32, 15);
            labelState.TabIndex = 5;
            labelState.Text = "state";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(474, 412);
            label1.Name = "label1";
            label1.Size = new Size(121, 15);
            label1.TabIndex = 6;
            label1.Text = "(arrow keys work too)";
            // 
            // buttonYouPlay
            // 
            buttonYouPlay.Location = new Point(91, 408);
            buttonYouPlay.Name = "buttonYouPlay";
            buttonYouPlay.Size = new Size(75, 23);
            buttonYouPlay.TabIndex = 7;
            buttonYouPlay.Text = "You Play";
            toolTip1.SetToolTip(buttonYouPlay, "Click this, and see if you can reach 500.");
            buttonYouPlay.UseVisualStyleBackColor = true;
            buttonYouPlay.Click += ButtonYouPlay_Click;
            // 
            // FormCartPoleGym
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(603, 436);
            Controls.Add(buttonYouPlay);
            Controls.Add(label1);
            Controls.Add(labelState);
            Controls.Add(buttonRight);
            Controls.Add(buttonLeft);
            Controls.Add(buttonAIPlay);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormCartPoleGym";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cart-Pole-Env";
            KeyDown += Form1_KeyDown;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Button buttonAIPlay;
        private Button buttonLeft;
        private Button buttonRight;
        private System.Windows.Forms.Timer timerMove;
        private Label labelState;
        private Label label1;
        private ToolTip toolTip1;
        private Button buttonYouPlay;
    }
}
