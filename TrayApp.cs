using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace BinaryReceptorWindows;

public class TrayApp : ApplicationContext
{
    private const string TrayIconResource = "BinaryReceptorWindows.Assets.icon.ico";
    private const string BinaryReceptorName = "BinaryReceptor";
    private string localUrl = "http://localhost:24234";
    private string exePath = "BinaryReceptor//BinaryReceptor.exe";
    private Mode mode = Mode.BLUETOOTH;
    private NotifyIcon trayIcon;
    private ToolStripMenuItem menuItemMode;
    private ToolStripMenuItem menuItemSwitch;
    private ToolStripMenuItem menuItemStart;
    private ToolStripMenuItem menuItemStop;
    private ToolStripMenuItem menuItemShow;
    private ToolStripMenuItem menuItemExit;
    
    public TrayApp()
    {
        CreateTrayIcon();
    }

    private void CreateTrayIcon()
    {
        menuItemMode = new ToolStripMenuItem("Mode: Bluetooth", null, Switch);
        menuItemSwitch = new ToolStripMenuItem("Switch Modes", null, Switch);
        menuItemStart = new ToolStripMenuItem("Start", null, Start);
        menuItemStop = new ToolStripMenuItem("Stop", null, Stop);
        menuItemShow = new ToolStripMenuItem("Show", null, Show);
        menuItemExit = new ToolStripMenuItem("Exit", null, Exit);

        ContextMenuStrip contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(menuItemMode);
        contextMenu.Items.Add(menuItemSwitch);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(menuItemStart);
        contextMenu.Items.Add(menuItemStop);
        contextMenu.Items.Add(menuItemShow);
        contextMenu.Items.Add(menuItemExit);

        contextMenu.Opening += new CancelEventHandler(ContextMenuOnPopup);
        using var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(TrayIconResource);
        trayIcon = new NotifyIcon() 
        {
            Icon = new Icon(iconStream), 
            ContextMenuStrip = contextMenu, 
            Visible = true, 
            Text = BinaryReceptorName 
        };
    }
    private void Switch(object sender, EventArgs e)
    {
        Stop(null, null);
        if (mode == Mode.BLUETOOTH)
            mode = Mode.HTTP;
        else
            mode = Mode.BLUETOOTH;
    }
    private void Start(object sender, EventArgs e)
    {
        string modeString = "bluetooth";
        if (mode == Mode.BLUETOOTH)
            modeString = "bluetooth";
        else
            modeString = "http";

        Process p = new Process();
        p.StartInfo.FileName = exePath;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        p.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
        p.StartInfo.Arguments = modeString;
        p.Start();
    }
    private void Stop(object sender, EventArgs e)
    {
        Process? process = Process.GetProcessesByName(BinaryReceptorName).FirstOrDefault();
        if (process == null)
        {
            return;
        }

        if (!process.CloseMainWindow())
        {
            process.Kill();
        }
    }
    private void Show(object sender, EventArgs e)
    {
        // hack because of this: https://github.com/dotnet/corefx/issues/10361
        string url = localUrl.Replace("&", "^&");
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    private void Exit(object sender, EventArgs e)
    {
        Stop(null, null);
        trayIcon.Visible = false;
        Application.Exit();
    }
    private void ContextMenuOnPopup(object sender, EventArgs e)
    {
        bool exeRunning = Process.GetProcessesByName(BinaryReceptorName).Length > 0;
        bool running = exeRunning;
        bool stopped = !exeRunning;

        menuItemStart.Enabled = stopped;
        menuItemShow.Enabled = running;
        menuItemStop.Enabled = running;

        if (mode == Mode.BLUETOOTH)
            menuItemMode.Text = "Mode: Bluetooth";
        else
            menuItemMode.Text = "Mode: HTTP";
    } 
}