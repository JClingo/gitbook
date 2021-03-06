﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Alphora.Dataphor.Dataphoria
{
	public partial class ProcessesView : UserControl
	{
		public ProcessesView()
		{
			InitializeComponent();
		}

		private IDataphoria _dataphoria;
		public IDataphoria Dataphoria
		{
			get { return _dataphoria; }
			set
			{
				if (_dataphoria != value)
				{
					if (_dataphoria != null)
					{
						_dataphoria.Disconnected -= new EventHandler(FDataphoria_Disconnected);
						_dataphoria.Connected -= new EventHandler(FDataphoria_Connected);
						_dataphoria.Debugger.PropertyChanged -= Debugger_PropertyChanged;
					}
					_dataphoria = value;
					if (_dataphoria != null)
					{
						_dataphoria.Disconnected += new EventHandler(FDataphoria_Disconnected);
						_dataphoria.Connected += new EventHandler(FDataphoria_Connected);
						_dataphoria.Debugger.PropertyChanged += Debugger_PropertyChanged;
						if (_dataphoria.IsConnected)
							FDataphoria_Connected(this, EventArgs.Empty);
					}
				}
			}
		}

		private bool _supressDebuggerChange;
		
		private void Debugger_PropertyChanged(object sender, string[] propertyNames)
		{
			if (!_supressDebuggerChange && Array.Exists<string>(propertyNames, (string AItem) => { return AItem == "IsStarted" || AItem == "IsPaused"; }))
				RefreshDataView();
		}

		private void FDataphoria_Disconnected(object sender, EventArgs e)
		{
			try
			{
				FProcessDataView.Close();
				FProcessDataView.Session = null;
				UpdateButtonsEnabled();
			}
			catch (Exception exception)
			{
				_dataphoria.Warnings.AppendError(null, exception, false);
			}
		}
		
		private void FDataphoria_Connected(object sender, EventArgs e)
		{
			FProcessDataView.Session = _dataphoria.DataSession;
			try
			{
				FProcessDataView.Open();
				UpdateButtonsEnabled();
			}
			catch (Exception exception)
			{
				_dataphoria.Warnings.AppendError(null, exception, false);
			}
		}

		private void FAttachButton_Click(object sender, EventArgs e)
		{
			if (FProcessDataView.Active && !FProcessDataView.IsEmpty())
			{
				_supressDebuggerChange = true;
				try
				{
					_dataphoria.Debugger.AttachProcess(FProcessDataView["ID"].AsInt32);
				}
				finally
				{
					_supressDebuggerChange = false;
				}
				RefreshDataView();
			}
		}

		private void FDetachButton_Click(object sender, EventArgs e)
		{
			if (FProcessDataView.Active && !FProcessDataView.IsEmpty())
			{
				_supressDebuggerChange = true;
				try
				{
					_dataphoria.Debugger.DetachProcess(FProcessDataView["ID"].AsInt32);
				}
				finally
				{
					_supressDebuggerChange = false;
				}
				RefreshDataView();
			}
		}

		private void FRefreshButton_Click(object sender, EventArgs e)
		{
			RefreshDataView();
		}

		private void RefreshDataView()
		{
			if (FProcessDataView.Active)
				FProcessDataView.Refresh();
		}

		private void FSessionDataView_DataChanged(object sender, EventArgs e)
		{
			UpdateButtonsEnabled();
		}

		private void UpdateButtonsEnabled()
		{
			var hasRow = FProcessDataView.Active && !FProcessDataView.IsEmpty();
			var isAttached = hasRow && (bool)FProcessDataView["IsAttached"];
			FAttachButton.Enabled = hasRow && !isAttached;
			FAttachContextMenuItem.Enabled = hasRow && !isAttached;
			FDetachButton.Enabled = hasRow && isAttached;
			FDetachContextMenuItem.Enabled = hasRow && isAttached;
		}
	}
}
