namespace CosmosDbUtility.GUI;

partial class Form1
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
				this.lblDatabaseId = new System.Windows.Forms.Label();
				this.clbContainerNames = new System.Windows.Forms.CheckedListBox();
				this.lblContainers = new System.Windows.Forms.Label();
				this.btnBackup = new System.Windows.Forms.Button();
				this.lbDatabases = new System.Windows.Forms.ListBox();
				this.SuspendLayout();
				// 
				// lblDatabaseId
				// 
				this.lblDatabaseId.AutoSize = true;
				this.lblDatabaseId.Location = new System.Drawing.Point(62, 13);
				this.lblDatabaseId.Name = "lblDatabaseId";
				this.lblDatabaseId.Size = new System.Drawing.Size(69, 15);
				this.lblDatabaseId.TabIndex = 0;
				this.lblDatabaseId.Text = "Database ID";
				// 
				// clbContainerNames
				// 
				this.clbContainerNames.FormattingEnabled = true;
				this.clbContainerNames.Location = new System.Drawing.Point(142, 39);
				this.clbContainerNames.Name = "clbContainerNames";
				this.clbContainerNames.Size = new System.Drawing.Size(292, 58);
				this.clbContainerNames.TabIndex = 2;
				// 
				// lblContainers
				// 
				this.lblContainers.AutoSize = true;
				this.lblContainers.Location = new System.Drawing.Point(34, 39);
				this.lblContainers.Name = "lblContainers";
				this.lblContainers.Size = new System.Drawing.Size(97, 15);
				this.lblContainers.TabIndex = 3;
				this.lblContainers.Text = "Container names";
				// 
				// btnBackup
				// 
				this.btnBackup.Location = new System.Drawing.Point(467, 10);
				this.btnBackup.Name = "btnBackup";
				this.btnBackup.Size = new System.Drawing.Size(292, 87);
				this.btnBackup.TabIndex = 4;
				this.btnBackup.Text = "Backup";
				this.btnBackup.UseVisualStyleBackColor = true;
				this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
				// 
				// lbDatabases
				// 
				this.lbDatabases.FormattingEnabled = true;
				this.lbDatabases.ItemHeight = 15;
				this.lbDatabases.Location = new System.Drawing.Point(141, 12);
				this.lbDatabases.Name = "lbDatabases";
				this.lbDatabases.Size = new System.Drawing.Size(293, 19);
				this.lbDatabases.TabIndex = 5;
				this.lbDatabases.SelectedIndexChanged += new System.EventHandler(this.lbDatabase_SelectedIndexChanged);
				// 
				// Form1
				// 
				this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
				this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
				this.ClientSize = new System.Drawing.Size(797, 109);
				this.Controls.Add(this.lbDatabases);
				this.Controls.Add(this.btnBackup);
				this.Controls.Add(this.lblContainers);
				this.Controls.Add(this.clbContainerNames);
				this.Controls.Add(this.lblDatabaseId);
				this.Name = "Form1";
				this.Text = "Form1";
				this.ResumeLayout(false);
				this.PerformLayout();

	 }

	 #endregion

	 private Label lblDatabaseId;
	 private CheckedListBox clbContainerNames;
	 private Label lblContainers;
	 private Button btnBackup;
	 private ListBox lbDatabases;
}
