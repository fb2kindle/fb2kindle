﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryCleaner {
  public partial class MainForm : Form {
    private readonly Cleaner cleaner;
    // private readonly string _logFileName;

    public MainForm() {
      InitializeComponent();

      var module = Process.GetCurrentProcess().MainModule;
      if (module != null)
        Icon = Icon.ExtractAssociatedIcon(module.FileName);

      var assembly = Assembly.GetExecutingAssembly();
      // _logFileName = Path.ChangeExtension(assembly.Location, ".log");
      var asm = assembly.GetName();
      var mainTitleText = $"{asm.Name} Version: {asm.Version.ToString(3)}";
      var timer = new Timer { Interval = 1000, Enabled = true };
      timer.Tick += (sender, args) => {
        var dt = DateTime.Now;
        var build = dt.Subtract(new DateTime(2000, 1, 1)).Days;
        var revision = (dt.Second + dt.Minute * 60 + dt.Hour * 60 * 60) / 2;
        Text = $"{mainTitleText}; Now: {build}.{revision}";
      };

      cleaner = new Cleaner(null);
      cleaner.OnStateChanged += AddToLog;

      clsGenres.Items.Clear();
      var genres = GenresListContainer.GetDefaultItems();
      foreach (var genre in genres) {
        clsGenres.Items.Add(genre, genre.Selected);
      }

      cbxUpdateHashes.Checked = cleaner.UpdateHashInfo;
      cbxRemoveDeleted.Checked = cleaner.RemoveDeleted;
      cbxRemoveMissedArchives.Checked = cleaner.RemoveMissingArchivesFromDb;

      txtDatabase.Text = Path.Combine(Path.GetDirectoryName(assembly.Location), "myrulib.db");
      // txtLog.Font = new Font("Verdana", 10, FontStyle.Regular);
    }

    #region GUI helper methods

    private void AddToLog(string message, Cleaner.StateKind state) {
      if (InvokeRequired) {
        Invoke(new Action<string, Cleaner.StateKind>(AddToLog), message, state);
        return;
      }

      txtLog.AppendLine($"{DateTime.Now:T} - ", Color.LightGray);
      switch (state) {
        case Cleaner.StateKind.Error:
          txtLog.AppendLine(message, Color.Red, false);
          break;
        case Cleaner.StateKind.Warning:
          txtLog.AppendLine(message, Color.Gold, false);
          break;
        case Cleaner.StateKind.Message:
          txtLog.AppendLine(message, Color.LimeGreen, false);
          break;
        case Cleaner.StateKind.Info:
          txtLog.AppendLine(message, Color.CornflowerBlue, false);
          break;
        default:
          txtLog.AppendLine(message, txtLog.ForeColor, false);
          break;
      }

      // if (!string.IsNullOrEmpty(_logFileName))
      //   File.AppendAllText(_logFileName, $"{DateTime.Now:T} - {message}\n");
      txtLog.ScrollToCaret();
      Application.DoEvents();
    }

    private void btnBrowse_Click(object sender, EventArgs e) {
      var dlg = new OpenFileDialog { CheckFileExists = true, Filter = "Database file (*.db)|*.db" };
      // dlg.InitialDirectory = Environment.CurrentDirectory; 
      dlg.RestoreDirectory = true; 
      if (dlg.ShowDialog() != DialogResult.OK) return;
      txtDatabase.Text = dlg.FileName;
    }

    private void btnBrowseOutput_Click(object sender, EventArgs e) {
      var dlg = new FolderBrowserDialog();
      if (dlg.ShowDialog() != DialogResult.OK) return;
      txtOutput.Text = dlg.SelectedPath;
    }

    private void btnDeletedFile_Click(object sender, EventArgs e) {
      var dlg = new OpenFileDialog {
        CheckFileExists = true,
        Filter = "Csv file (*.csv)|*.csv|Text file (*.txt)|*.txt|All files (*.*)|*.*"
      };
      if (dlg.ShowDialog() != DialogResult.OK) return;
      txtDeletedFile.Text = dlg.FileName;
    }

    private void btnAllGenres_Click(object sender, EventArgs e) {
      for (var i = 0; i < clsGenres.Items.Count; i++)
        clsGenres.SetItemChecked(i, true);
    }

    private void btnNoneGenres_Click(object sender, EventArgs e) {
      for (var i = 0; i < clsGenres.Items.Count; i++)
        clsGenres.SetItemChecked(i, false);
    }

    #endregion GUI helper methods

    private void ProcessCleanupTasks(bool analyzeOnly) {
      var startedTime = DateTime.Now;
      SetStartedState();
      Task.Factory.StartNew(async () => {
        try {
          //get selected genres list
          var genresToRemove = clsGenres.CheckedItems.Cast<Genres>().Select(s => s.Code).ToArray();
          cleaner.GenresToRemove = genresToRemove;
          cleaner.DatabasePath = txtDatabase.Text;
          cleaner.ArchivesOutputPath = txtOutput.Text;
          cleaner.UpdateHashInfo = cbxUpdateHashes.Checked;
          cleaner.RemoveDeleted = cbxRemoveDeleted.Checked;
          cleaner.RemoveNotRegisteredFilesFromZip = cbxRemoveDeleted.Checked;
          cleaner.RemoveMissingArchivesFromDb = cbxRemoveMissedArchives.Checked; // && !analyzeOnly;
          cleaner.MinFilesToUpdateZip = (int)edtMinFilesToSave.Value;
          cleaner.FileWithDeletedBooksIds = txtDeletedFile.Text;

          if (!await cleaner.CheckParameters().ConfigureAwait(false)) {
            AddToLog("Please check input parameters and start again!", Cleaner.StateKind.Warning);
            SetFinishedState(startedTime);
            return;
          }

          await cleaner.CalculateStats().ConfigureAwait(false);

          if (!analyzeOnly) {
            await cleaner.CompressLibrary().ConfigureAwait(false);
            AddToLog("Finished!", Cleaner.StateKind.Log);
          }

          SetFinishedState(startedTime);
        }
        catch (Exception ex) {
          AddToLog(ex.Message, Cleaner.StateKind.Error);
          SetFinishedState(startedTime);
        }
      });
    }

    private void btnAnalyze_Click(object sender, EventArgs e) {
      ProcessCleanupTasks(true);
    }

    private void btnStart_Click(object sender, EventArgs e) {
      ProcessCleanupTasks(false);
    }

    private void SetStartedState() {
      tabControlConfig.Visible = false;
      btnAnalyze.Enabled = false;
      btnStart.Enabled = false;
      Cursor = Cursors.WaitCursor;
    }

    private void SetFinishedState(DateTime startedTime) {
      if (InvokeRequired) {
        Invoke(new Action<DateTime>(SetFinishedState), startedTime);
        return;
      }

      var timeWasted = DateTime.Now - startedTime;
      AddToLog($"Time wasted: {timeWasted:G}", Cleaner.StateKind.Log);
      tabControlConfig.Visible = true;
      btnAnalyze.Enabled = true;
      btnStart.Enabled = true;
      Cursor = Cursors.Default;
    }

  }
}