namespace Cart
{
    partial class FormPoleDirection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPoleDirection));
            pictureBoxVideoDisplay = new PictureBox();
            buttonTrain = new Button();
            labelFrameNumber = new Label();
            numericUpDown1 = new NumericUpDown();
            labelNNresult = new Label();
            label1 = new Label();
            listBoxNNnegativeAngleOutput = new ListBox();
            listBoxNNpositiveAngleOutput = new ListBox();
            label2 = new Label();
            label3 = new Label();
            labelStateOfNN = new Label();
            pictureBoxNNmap = new PictureBox();
            pictureBoxNNfiring = new PictureBox();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxVideoDisplay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxNNmap).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxNNfiring).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxVideoDisplay
            // 
            pictureBoxVideoDisplay.Location = new Point(6, 34);
            pictureBoxVideoDisplay.Name = "pictureBoxVideoDisplay";
            pictureBoxVideoDisplay.Size = new Size(320, 260);
            pictureBoxVideoDisplay.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxVideoDisplay.TabIndex = 0;
            pictureBoxVideoDisplay.TabStop = false;
            // 
            // buttonTrain
            // 
            buttonTrain.Location = new Point(7, 391);
            buttonTrain.Name = "buttonTrain";
            buttonTrain.Size = new Size(75, 34);
            buttonTrain.TabIndex = 27;
            buttonTrain.Text = "Train";
            buttonTrain.UseVisualStyleBackColor = true;
            buttonTrain.Click += ButtonTrain_Click;
            // 
            // labelFrameNumber
            // 
            labelFrameNumber.Anchor = AnchorStyles.None;
            labelFrameNumber.Location = new Point(773, -77);
            labelFrameNumber.Name = "labelFrameNumber";
            labelFrameNumber.Size = new Size(206, 23);
            labelFrameNumber.TabIndex = 12;
            labelFrameNumber.Text = "frame # of #";
            labelFrameNumber.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numericUpDown1
            // 
            numericUpDown1.DecimalPlaces = 2;
            numericUpDown1.Location = new Point(118, 6);
            numericUpDown1.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 24, 0, 0, int.MinValue });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(61, 23);
            numericUpDown1.TabIndex = 29;
            numericUpDown1.ValueChanged += NudAngle_ValueChanged;
            // 
            // labelNNresult
            // 
            labelNNresult.Font = new Font("Consolas", 20F);
            labelNNresult.Location = new Point(7, 297);
            labelNNresult.Name = "labelNNresult";
            labelNNresult.Size = new Size(319, 38);
            labelNNresult.TabIndex = 30;
            labelNNresult.Text = "NN output";
            labelNNresult.TextAlign = ContentAlignment.TopCenter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(71, 10);
            label1.Name = "label1";
            label1.Size = new Size(41, 15);
            label1.TabIndex = 31;
            label1.Text = "Angle:";
            // 
            // listBoxNNnegativeAngleOutput
            // 
            listBoxNNnegativeAngleOutput.FormattingEnabled = true;
            listBoxNNnegativeAngleOutput.ItemHeight = 15;
            listBoxNNnegativeAngleOutput.Location = new Point(334, 7);
            listBoxNNnegativeAngleOutput.Name = "listBoxNNnegativeAngleOutput";
            listBoxNNnegativeAngleOutput.Size = new Size(211, 424);
            listBoxNNnegativeAngleOutput.TabIndex = 32;
            // 
            // listBoxNNpositiveAngleOutput
            // 
            listBoxNNpositiveAngleOutput.FormattingEnabled = true;
            listBoxNNpositiveAngleOutput.ItemHeight = 15;
            listBoxNNpositiveAngleOutput.Location = new Point(552, 7);
            listBoxNNpositiveAngleOutput.Name = "listBoxNNpositiveAngleOutput";
            listBoxNNpositiveAngleOutput.Size = new Size(211, 424);
            listBoxNNpositiveAngleOutput.TabIndex = 33;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Variable Small", 9F);
            label2.Location = new Point(6, 335);
            label2.Name = "label2";
            label2.Size = new Size(148, 16);
            label2.TabIndex = 34;
            label2.Text = "0=left,1=right (or upright) ";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Variable Small", 9F);
            label3.Location = new Point(177, 335);
            label3.Name = "label3";
            label3.Size = new Size(148, 16);
            label3.TabIndex = 35;
            label3.Text = "Raw: <0.5 left, >=0.5 right";
            // 
            // labelStateOfNN
            // 
            labelStateOfNN.AutoSize = true;
            labelStateOfNN.Font = new Font("Segoe UI Variable Display Semib", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelStateOfNN.Location = new Point(88, 400);
            labelStateOfNN.Name = "labelStateOfNN";
            labelStateOfNN.Size = new Size(80, 16);
            labelStateOfNN.TabIndex = 36;
            labelStateOfNN.Text = "**untrained**";
            // 
            // pictureBoxNNmap
            // 
            pictureBoxNNmap.Location = new Point(771, 20);
            pictureBoxNNmap.Name = "pictureBoxNNmap";
            pictureBoxNNmap.Size = new Size(180, 130);
            pictureBoxNNmap.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxNNmap.TabIndex = 37;
            pictureBoxNNmap.TabStop = false;
            // 
            // pictureBoxNNfiring
            // 
            pictureBoxNNfiring.Location = new Point(771, 258);
            pictureBoxNNfiring.Name = "pictureBoxNNfiring";
            pictureBoxNNfiring.Size = new Size(180, 130);
            pictureBoxNNfiring.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxNNfiring.TabIndex = 38;
            pictureBoxNNfiring.TabStop = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Bold);
            label4.Location = new Point(766, 4);
            label4.Name = "label4";
            label4.Size = new Size(95, 16);
            label4.TabIndex = 39;
            label4.Text = "Neural network";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Bold);
            label5.Location = new Point(767, 239);
            label5.Name = "label5";
            label5.Size = new Size(88, 16);
            label5.TabIndex = 40;
            label5.Text = "Neurons firing";
            // 
            // label6
            // 
            label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label6.Location = new Point(7, 436);
            label6.Name = "label6";
            label6.Size = new Size(957, 36);
            label6.TabIndex = 41;
            label6.Text = resources.GetString("label6.Text");
            // 
            // label7
            // 
            label7.Location = new Point(768, 391);
            label7.Name = "label7";
            label7.Size = new Size(184, 36);
            label7.TabIndex = 42;
            label7.Text = "Shows the neuron response to the image.";
            // 
            // label8
            // 
            label8.Location = new Point(768, 156);
            label8.Name = "label8";
            label8.Size = new Size(182, 61);
            label8.TabIndex = 43;
            label8.Text = "Shows the neuron response to each individual pixel (achieved by testing the output for each pixel separately).";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(185, 10);
            label9.Name = "label9";
            label9.Size = new Size(48, 15);
            label9.TabIndex = 44;
            label9.Text = "degrees";
            // 
            // FormPoleDirection
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(960, 474);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(pictureBoxNNfiring);
            Controls.Add(pictureBoxNNmap);
            Controls.Add(labelStateOfNN);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(listBoxNNpositiveAngleOutput);
            Controls.Add(listBoxNNnegativeAngleOutput);
            Controls.Add(label1);
            Controls.Add(labelNNresult);
            Controls.Add(numericUpDown1);
            Controls.Add(buttonTrain);
            Controls.Add(labelFrameNumber);
            Controls.Add(pictureBoxVideoDisplay);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormPoleDirection";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AI learning to return 0 if pole is pointing left, 1 if pointing right (or upright)";
            Load += FormCartPoleGym_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxVideoDisplay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxNNmap).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxNNfiring).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBoxVideoDisplay;
        private Label labelFrameNumber;
        private Button buttonTrain;
        private NumericUpDown numericUpDown1;
        private Label labelNNresult;
        private Label label1;
        private ListBox listBoxNNnegativeAngleOutput;
        private ListBox listBoxNNpositiveAngleOutput;
        private Label label2;
        private Label label3;
        private Label labelStateOfNN;
        private PictureBox pictureBoxNNmap;
        private PictureBox pictureBoxNNfiring;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
    }
}
