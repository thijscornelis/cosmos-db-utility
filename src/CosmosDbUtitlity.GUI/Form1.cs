using CosmosDbUtility.GUI.Abstractions;

namespace CosmosDbUtility.GUI;

public partial class Form1 : Form
{
	 private readonly IFacade _facade;

	 public Form1(IFacade facade)
	 {
		  _facade = facade;
		  InitializeComponent();

		  lbDatabases.DataSource = _facade.GetDatabases();
	 }

	 private void btnBackup_Click(object sender, EventArgs e)
	 {
	 }

	 private async Task lbDatabase_SelectedIndexChanged(object sender, EventArgs e, CancellationToken cancellationToken = default)
	 {
		  clbContainerNames.ClearSelected();
		  clbContainerNames.DataSource = await _facade.GetContainersAsync(lbDatabases.SelectedItem.ToString());
	 }
}
