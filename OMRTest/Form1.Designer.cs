
namespace OMRTest
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.msg = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.UnreadableFiles = new System.Windows.Forms.Label();
            this.PrimaryPercent = new System.Windows.Forms.Label();
            this.PrimaryTotal = new System.Windows.Forms.Label();
            this.CurrentOperation = new System.Windows.Forms.Label();
            this.PrimaryValue = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(86, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File Compeleted:";
            // 
            // msg
            // 
            this.msg.AutoSize = true;
            this.msg.ForeColor = System.Drawing.Color.Red;
            this.msg.Location = new System.Drawing.Point(321, 236);
            this.msg.Name = "msg";
            this.msg.Size = new System.Drawing.Size(0, 13);
            this.msg.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(92, 173);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Current Operation Text:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(86, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Total files to process:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(86, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Percent Compele:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(86, 97);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Unreadable Files:";
            // 
            // UnreadableFiles
            // 
            this.UnreadableFiles.AutoSize = true;
            this.UnreadableFiles.Location = new System.Drawing.Point(234, 97);
            this.UnreadableFiles.Name = "UnreadableFiles";
            this.UnreadableFiles.Size = new System.Drawing.Size(0, 13);
            this.UnreadableFiles.TabIndex = 10;
            // 
            // PrimaryPercent
            // 
            this.PrimaryPercent.AutoSize = true;
            this.PrimaryPercent.Location = new System.Drawing.Point(234, 123);
            this.PrimaryPercent.Name = "PrimaryPercent";
            this.PrimaryPercent.Size = new System.Drawing.Size(0, 13);
            this.PrimaryPercent.TabIndex = 9;
            // 
            // PrimaryTotal
            // 
            this.PrimaryTotal.AutoSize = true;
            this.PrimaryTotal.Location = new System.Drawing.Point(234, 150);
            this.PrimaryTotal.Name = "PrimaryTotal";
            this.PrimaryTotal.Size = new System.Drawing.Size(0, 13);
            this.PrimaryTotal.TabIndex = 8;
            // 
            // CurrentOperation
            // 
            this.CurrentOperation.AutoSize = true;
            this.CurrentOperation.Location = new System.Drawing.Point(240, 173);
            this.CurrentOperation.Name = "CurrentOperation";
            this.CurrentOperation.Size = new System.Drawing.Size(0, 13);
            this.CurrentOperation.TabIndex = 7;
            // 
            // PrimaryValue
            // 
            this.PrimaryValue.AutoSize = true;
            this.PrimaryValue.Location = new System.Drawing.Point(234, 73);
            this.PrimaryValue.Name = "PrimaryValue";
            this.PrimaryValue.Size = new System.Drawing.Size(0, 13);
            this.PrimaryValue.TabIndex = 6;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(574, 410);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.UnreadableFiles);
            this.Controls.Add(this.PrimaryPercent);
            this.Controls.Add(this.PrimaryTotal);
            this.Controls.Add(this.CurrentOperation);
            this.Controls.Add(this.PrimaryValue);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.msg);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label msg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label UnreadableFiles;
        private System.Windows.Forms.Label PrimaryPercent;
        private System.Windows.Forms.Label PrimaryTotal;
        private System.Windows.Forms.Label CurrentOperation;
        private System.Windows.Forms.Label PrimaryValue;
        private System.Windows.Forms.Button button1;
    }
}

