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
            pictureBoxCartEnvironmentDisplay = new PictureBox();
            buttonAIPlay = new Button();
            buttonLeft = new Button();
            buttonRight = new Button();
            timerMove = new System.Windows.Forms.Timer(components);
            labelState = new Label();
            label1 = new Label();
            toolTip1 = new ToolTip(components);
            buttonYouPlay = new Button();
            buttonPrevFrame = new Button();
            buttonNextFrame = new Button();
            labelFrameByFrame = new Label();
            labelFrameNumber = new Label();
            pictureBoxFrame = new PictureBox();
            pictureBoxFrameDiff = new PictureBox();
            checkBoxMakeItEasier = new CheckBox();
            label2 = new Label();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCartEnvironmentDisplay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxFrameDiff).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxCartEnvironmentDisplay
            // 
            pictureBoxCartEnvironmentDisplay.Location = new Point(1, 23);
            pictureBoxCartEnvironmentDisplay.Name = "pictureBoxCartEnvironmentDisplay";
            pictureBoxCartEnvironmentDisplay.Size = new Size(600, 400);
            pictureBoxCartEnvironmentDisplay.TabIndex = 0;
            pictureBoxCartEnvironmentDisplay.TabStop = false;
            // 
            // buttonAIPlay
            // 
            buttonAIPlay.Location = new Point(527, 441);
            buttonAIPlay.Name = "buttonAIPlay";
            buttonAIPlay.Size = new Size(75, 52);
            buttonAIPlay.TabIndex = 1;
            buttonAIPlay.Text = "AI Play";
            toolTip1.SetToolTip(buttonAIPlay, "Click this, and weep. This is how a pro does it.");
            buttonAIPlay.UseVisualStyleBackColor = true;
            buttonAIPlay.Click += ButtonAIPlays_Click;
            // 
            // buttonLeft
            // 
            buttonLeft.Location = new Point(11, 448);
            buttonLeft.Name = "buttonLeft";
            buttonLeft.Size = new Size(75, 45);
            buttonLeft.TabIndex = 2;
            buttonLeft.Text = "Left";
            toolTip1.SetToolTip(buttonLeft, "Steer left (arrow key is easier).");
            buttonLeft.UseVisualStyleBackColor = true;
            buttonLeft.Click += ButtonLeft_Click;
            // 
            // buttonRight
            // 
            buttonRight.Location = new Point(88, 448);
            buttonRight.Name = "buttonRight";
            buttonRight.Size = new Size(75, 45);
            buttonRight.TabIndex = 3;
            buttonRight.Text = "Right";
            toolTip1.SetToolTip(buttonRight, "Steer right (arrow key is easier).");
            buttonRight.UseVisualStyleBackColor = true;
            buttonRight.Click += ButtonRight_Click;
            // 
            // timerMove
            // 
            timerMove.Interval = 20;
            timerMove.Tick += TimerMove_Tick;
            // 
            // labelState
            // 
            labelState.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelState.Location = new Point(1, -2);
            labelState.Name = "labelState";
            labelState.Size = new Size(600, 25);
            labelState.TabIndex = 5;
            labelState.Text = "state";
            labelState.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 430);
            label1.Name = "label1";
            label1.Size = new Size(121, 15);
            label1.TabIndex = 6;
            label1.Text = "(arrow keys work too)";
            // 
            // buttonYouPlay
            // 
            buttonYouPlay.Location = new Point(54, 499);
            buttonYouPlay.Name = "buttonYouPlay";
            buttonYouPlay.Size = new Size(75, 52);
            buttonYouPlay.TabIndex = 7;
            buttonYouPlay.Text = "Play";
            toolTip1.SetToolTip(buttonYouPlay, "Click this, and see if you can reach 500.");
            buttonYouPlay.UseVisualStyleBackColor = true;
            buttonYouPlay.Click += ButtonYouPlay_Click;
            // 
            // buttonPrevFrame
            // 
            buttonPrevFrame.FlatStyle = FlatStyle.Flat;
            buttonPrevFrame.Location = new Point(607, 152);
            buttonPrevFrame.Name = "buttonPrevFrame";
            buttonPrevFrame.Size = new Size(20, 23);
            buttonPrevFrame.TabIndex = 9;
            buttonPrevFrame.Text = "<";
            buttonPrevFrame.UseVisualStyleBackColor = true;
            buttonPrevFrame.Click += ButtonPreviousFrame_Click;
            // 
            // buttonNextFrame
            // 
            buttonNextFrame.FlatStyle = FlatStyle.Flat;
            buttonNextFrame.Location = new Point(1036, 128);
            buttonNextFrame.Name = "buttonNextFrame";
            buttonNextFrame.Size = new Size(20, 23);
            buttonNextFrame.TabIndex = 10;
            buttonNextFrame.Text = ">";
            buttonNextFrame.UseVisualStyleBackColor = true;
            buttonNextFrame.Click += ButtonNextFrame_Click;
            // 
            // labelFrameByFrame
            // 
            labelFrameByFrame.AutoSize = true;
            labelFrameByFrame.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelFrameByFrame.Location = new Point(629, 4);
            labelFrameByFrame.Name = "labelFrameByFrame";
            labelFrameByFrame.Size = new Size(82, 15);
            labelFrameByFrame.TabIndex = 11;
            labelFrameByFrame.Text = "Frame Replay";
            // 
            // labelFrameNumber
            // 
            labelFrameNumber.Anchor = AnchorStyles.None;
            labelFrameNumber.Location = new Point(825, -42);
            labelFrameNumber.Name = "labelFrameNumber";
            labelFrameNumber.Size = new Size(206, 23);
            labelFrameNumber.TabIndex = 12;
            labelFrameNumber.Text = "frame # of #";
            labelFrameNumber.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pictureBoxFrame
            // 
            pictureBoxFrame.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxFrame.Location = new Point(631, 22);
            pictureBoxFrame.Name = "pictureBoxFrame";
            pictureBoxFrame.Size = new Size(400, 297);
            pictureBoxFrame.TabIndex = 13;
            pictureBoxFrame.TabStop = false;
            // 
            // pictureBoxFrameDiff
            // 
            pictureBoxFrameDiff.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxFrameDiff.Location = new Point(631, 382);
            pictureBoxFrameDiff.Name = "pictureBoxFrameDiff";
            pictureBoxFrameDiff.Size = new Size(400, 110);
            pictureBoxFrameDiff.TabIndex = 14;
            pictureBoxFrameDiff.TabStop = false;
            // 
            // checkBoxMakeItEasier
            // 
            checkBoxMakeItEasier.AutoSize = true;
            checkBoxMakeItEasier.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            checkBoxMakeItEasier.Location = new Point(164, 510);
            checkBoxMakeItEasier.Name = "checkBoxMakeItEasier";
            checkBoxMakeItEasier.Size = new Size(238, 19);
            checkBoxMakeItEasier.TabIndex = 15;
            checkBoxMakeItEasier.Text = "Make it playable (5 moves per second)";
            checkBoxMakeItEasier.UseVisualStyleBackColor = true;
            checkBoxMakeItEasier.CheckedChanged += CheckBoxMakeItEasier_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(180, 529);
            label2.Name = "label2";
            label2.Size = new Size(427, 15);
            label2.TabIndex = 16;
            label2.Text = "The original computes tau for 0.02 = 50fps. At that speed humans are too slow...";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(631, 362);
            label3.Name = "label3";
            label3.Size = new Size(168, 15);
            label3.TabIndex = 17;
            label3.Text = "Last / This Frame Comparison";
            // 
            // FormCartPoleGym
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1065, 562);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(checkBoxMakeItEasier);
            Controls.Add(pictureBoxFrameDiff);
            Controls.Add(pictureBoxFrame);
            Controls.Add(labelFrameNumber);
            Controls.Add(labelFrameByFrame);
            Controls.Add(buttonNextFrame);
            Controls.Add(buttonPrevFrame);
            Controls.Add(buttonYouPlay);
            Controls.Add(label1);
            Controls.Add(labelState);
            Controls.Add(buttonRight);
            Controls.Add(buttonLeft);
            Controls.Add(buttonAIPlay);
            Controls.Add(pictureBoxCartEnvironmentDisplay);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormCartPoleGym";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cart-Pole-Env";
            KeyDown += Form1_KeyDown;
            ((System.ComponentModel.ISupportInitialize)pictureBoxCartEnvironmentDisplay).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxFrameDiff).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBoxCartEnvironmentDisplay;
        private Button buttonAIPlay;
        private Button buttonLeft;
        private Button buttonRight;
        private System.Windows.Forms.Timer timerMove;
        private Label labelState;
        private Label label1;
        private ToolTip toolTip1;
        private Button buttonYouPlay;
        private Button buttonPrevFrame;
        private Button buttonNextFrame;
        private Label labelFrameByFrame;
        private Label labelFrameNumber;
        private PictureBox pictureBoxFrame;
        private PictureBox pictureBoxFrameDiff;
        private CheckBox checkBoxMakeItEasier;
        private Label label2;
        private Label label3;
    }
}
