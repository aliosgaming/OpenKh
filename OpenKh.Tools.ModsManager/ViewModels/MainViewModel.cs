using OpenKh.Common;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using static OpenKh.Tools.ModsManager.Helpers;
using System.Text;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public interface IChangeModEnableState
    {
        void ModEnableStateChanged();
    }

    public class MainViewModel : BaseNotifyPropertyChanged, IChangeModEnableState
    {
        private static Version _version = Assembly.GetEntryAssembly()?.GetName()?.Version;
        private static string ApplicationName = Utilities.GetApplicationName();
        private static string ApplicationVersion = Utilities.GetApplicationVersion();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        private DebuggingWindow _debuggingWindow = new DebuggingWindow();
        private ModViewModel _selectedValue;
        private Pcsx2Injector _pcsx2Injector;
        private Process _runningProcess;
        private bool _isBuilding;
        private bool _pc;
        private bool _panaceaInstalled;
        private bool _devView;
        private string _launchGame = "kh2";
        private static string StoragePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private List<string> _supportedGames = new List<string>()
        {
            "kh2",
            "kh1",
            "bbs",
            "Recom"
        };
        private int _wizardVersionNumber = 2;
        private string[] executable = new string[]
        {
            "KINGDOM HEARTS II FINAL MIX.exe",
            "KINGDOM HEARTS FINAL MIX.exe",
            "KINGDOM HEARTS Birth by Sleep FINAL MIX.exe",
            "KINGDOM HEARTS Re_Chain of Memories.exe"
        };
        private int launchExecutable = 0;

        private const string RAW_FILES_FOLDER_NAME = "raw";
        private const string ORIGINAL_FILES_FOLDER_NAME = "original";
        private const string REMASTERED_FILES_FOLDER_NAME = "remastered";

        public string Title => ApplicationName;
        public string CurrentVersion => ApplicationVersion;
        public ObservableCollection<ModViewModel> ModsList { get; set; }
        public RelayCommand ExitCommand { get; set; }
        public RelayCommand AddModCommand { get; set; }
        public RelayCommand RemoveModCommand { get; set; }
        public RelayCommand OpenModFolderCommand { get; set; }
        public RelayCommand MoveUp { get; set; }
        public RelayCommand MoveDown { get; set; }
        public RelayCommand BuildCommand { get; set; }
        public RelayCommand PatchCommand { get; set; }
        public RelayCommand RestoreCommand { get; set; }
        public RelayCommand RunCommand { get; set; }
        public RelayCommand BuildAndRunCommand { get; set; }
        public RelayCommand StopRunningInstanceCommand { get; set; }
        public RelayCommand WizardCommand { get; set; }
        public RelayCommand OpenLinkCommand { get; set; }

        public ModViewModel SelectedValue
        {
            get => _selectedValue;
            set
            {
                _selectedValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsModSelected));
                OnPropertyChanged(nameof(IsModInfoVisible));
                OnPropertyChanged(nameof(IsModUnselectedMessageVisible));
                OnPropertyChanged(nameof(MoveUp));
                OnPropertyChanged(nameof(MoveDown));
                OnPropertyChanged(nameof(AddModCommand));
                OnPropertyChanged(nameof(RemoveModCommand));
                OnPropertyChanged(nameof(OpenModFolderCommand));
            }
        }

        public bool IsModSelected => SelectedValue != null;

        public Visibility IsModInfoVisible => IsModSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsModUnselectedMessageVisible => !IsModSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PatchVisible => PC && !PanaceaInstalled || PC && DevView ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ModLoader => !PC || PanaceaInstalled ? Visibility.Visible : Visibility.Collapsed;
        public Visibility notPC => !PC ? Visibility.Visible : Visibility.Collapsed;
        public Visibility isPC => PC ? Visibility.Visible : Visibility.Collapsed;

        public bool DevView
        {
            get => _devView;
            set
            {
                _devView = value;
                ConfigurationService.DevView = DevView;
                OnPropertyChanged(nameof(PatchVisible));
            }
        }
        public bool PanaceaInstalled
        {
            get => _panaceaInstalled;
            set
            {
                _panaceaInstalled = value;
                OnPropertyChanged(nameof(PatchVisible));
                OnPropertyChanged(nameof(ModLoader));
            }
        }

        public bool PC
        {
            get => _pc;
            set
            {
                _pc = value;
                OnPropertyChanged(nameof(PC));
                OnPropertyChanged(nameof(ModLoader));
                OnPropertyChanged(nameof(PatchVisible));
                OnPropertyChanged(nameof(notPC));
                OnPropertyChanged(nameof(isPC));
            }
        }  

        public int GametoLaunch
        {
            get
            {
                switch (_launchGame)
                {
                    case "kh2":
                        launchExecutable = 0;
                        return 0;
                    case "kh1":
                        launchExecutable = 1;
                        return 1;
                    case "bbs":
                        launchExecutable = 2;
                        return 2;
                    case "Recom":
                        launchExecutable = 3;
                        return 3;
                    default:
                        launchExecutable = 0;
                        return 0;
                }
            }
            set
            {
                launchExecutable = value;
                switch (value)
                {
                    case 0:
                        _launchGame = "kh2";
                        ConfigurationService.LaunchGame = "kh2";
                        break;
                    case 1:
                        _launchGame = "kh1";
                        ConfigurationService.LaunchGame = "kh1";
                        break;
                    case 2:
                        _launchGame = "bbs";
                        ConfigurationService.LaunchGame = "bbs";
                        break;
                    case 3:
                        _launchGame = "Recom";
                        ConfigurationService.LaunchGame = "Recom";
                        break;
                    default:
                        _launchGame = "kh2";
                        ConfigurationService.LaunchGame = "kh2";
                        break;
                }
                ReloadModsList();
            }
        }

        public bool IsBuilding
        {
            get => _isBuilding;
            set
            {
                _isBuilding = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(BuildCommand));
                    OnPropertyChanged(nameof(BuildAndRunCommand));
                });
            }
        }

        public bool IsRunning => _runningProcess != null;

        public MainViewModel()
        {
            if (ConfigurationService.GameEdition == 2)
            {
                PC = true;
                PanaceaInstalled = ConfigurationService.PanaceaInstalled;
                DevView = ConfigurationService.DevView;
            }
            else
                PC = false;
            if (_supportedGames.Contains(ConfigurationService.LaunchGame) && PC)
                _launchGame = ConfigurationService.LaunchGame;
            else
                ConfigurationService.LaunchGame = _launchGame;

            Log.OnLogDispatch += (long ms, string tag, string message) =>
                _debuggingWindow.Log(ms, tag, message);

            ReloadModsList();
            SelectedValue = ModsList.FirstOrDefault();

            ExitCommand = new RelayCommand(_ => Window.Close());
            AddModCommand = new RelayCommand(_ =>
            {
                var view = new InstallModView();
                if (view.ShowDialog() != true)
                    return;

                Task.Run(async () =>
                {
                    InstallModProgressWindow progressWindow = null;
                    try
                    {
                        var name = view.RepositoryName;
                        var isZipFile = view.IsZipFile;
                        progressWindow = Application.Current.Dispatcher.Invoke(() =>
                        {
                            var progressWindow = new InstallModProgressWindow
                            {
                                ModName = isZipFile ? Path.GetFileName(name) : name,
                                ProgressText = "Initializing",
                                ShowActivated = true
                            };
                            progressWindow.Show();
                            return progressWindow;
                        });

                        await ModsService.InstallMod(name, isZipFile, progress =>
                        {
                            Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressText = progress);
                        }, nProgress =>
                        {
                            Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressValue = nProgress);
                        });

                        var actualName = isZipFile ? Path.GetFileNameWithoutExtension(name) : name;
                        var mod = ModsService.GetMods(new string[] { actualName }).First();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            progressWindow.Close();
                            ModsList.Insert(0, Map(mod));
                            SelectedValue = ModsList[0];
                        });
                    }
                    catch (Exception ex)
                    {
                        Handle(ex);
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() => progressWindow?.Close());
                    }
                });
            }, _ => true);
            RemoveModCommand = new RelayCommand(_ =>
            {
                var mod = SelectedValue;
                if (Question($"Do you want to delete the mod '{mod.Source}'?", $"Remove mod {mod.Source}"))
                {
                    Handle(() =>
                    {
                        foreach (var filePath in Directory.GetFiles(mod.Path, "*", SearchOption.AllDirectories))
                        {
                            var attributes = File.GetAttributes(filePath);
                            if (attributes.HasFlag(FileAttributes.ReadOnly))
                                File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                        }

                        Directory.Delete(mod.Path, true);
                        ReloadModsList();
                    });
                }
            }, _ => IsModSelected);
            OpenModFolderCommand = new RelayCommand(_ =>
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = SelectedValue.Path,
                    UseShellExecute = true
                });
            }, _ => IsModSelected);
            MoveUp = new RelayCommand(_ => MoveSelectedModUp(), _ => CanSelectedModMoveUp());
            MoveDown = new RelayCommand(_ => MoveSelectedModDown(), _ => CanSelectedModMoveDown());
            BuildCommand = new RelayCommand(async _ =>
            {
                ResetLogWindow();
                await BuildPatches(false);
                CloseAllWindows();
            }, _ => !IsBuilding);

            PatchCommand = new RelayCommand(async (fastMode) =>
            {
                ResetLogWindow();
                await BuildPatches(Convert.ToBoolean(fastMode));
                await PatchGame(Convert.ToBoolean(fastMode));
                CloseAllWindows();
            }, _ => !IsBuilding);

            RunCommand = new RelayCommand(async _ =>
            {
                CloseRunningProcess();
                ResetLogWindow();
                await RunGame();
            });

            RestoreCommand = new RelayCommand(async (patched) =>
            {
                ResetLogWindow();
                await RestoreGame(Convert.ToBoolean(patched));
                CloseAllWindows();
            });
            BuildAndRunCommand = new RelayCommand(async _ =>
            {
                CloseRunningProcess();
                ResetLogWindow();
                if (await BuildPatches(false))
                    await RunGame();
            }, _ => !IsBuilding);
            StopRunningInstanceCommand = new RelayCommand(_ =>
            {
                CloseRunningProcess();
                ResetLogWindow();
            }, _ => IsRunning);
            WizardCommand = new RelayCommand(_ =>
            {
                var dialog = new SetupWizardWindow()
                {
                    ConfigGameEdition = ConfigurationService.GameEdition,
                    ConfigGameDataLocation = ConfigurationService.GameDataLocation,
                    ConfigIsoLocation = ConfigurationService.IsoLocation,
                    ConfigOpenKhGameEngineLocation = ConfigurationService.OpenKhGameEngineLocation,
                    ConfigPcsx2Location = ConfigurationService.Pcsx2Location,
                    ConfigPcReleaseLocation = ConfigurationService.PcReleaseLocation,
                    ConfigPcReleaseLanguage = ConfigurationService.PcReleaseLanguage,
                    ConfigRegionId = ConfigurationService.RegionId,
                    ConfigPanaceaInstalled = ConfigurationService.PanaceaInstalled,
                    ConfigIsEGSVersion = ConfigurationService.IsEGSVersion,
                };
                if (dialog.ShowDialog() == true)
                {
                    ConfigurationService.GameEdition = dialog.ConfigGameEdition;
                    ConfigurationService.GameDataLocation = dialog.ConfigGameDataLocation;
                    ConfigurationService.IsoLocation = dialog.ConfigIsoLocation;
                    ConfigurationService.OpenKhGameEngineLocation = dialog.ConfigOpenKhGameEngineLocation;
                    ConfigurationService.Pcsx2Location = dialog.ConfigPcsx2Location;
                    ConfigurationService.PcReleaseLocation = dialog.ConfigPcReleaseLocation;
                    ConfigurationService.RegionId = dialog.ConfigRegionId;
                    ConfigurationService.PanaceaInstalled = dialog.ConfigPanaceaInstalled;
                    ConfigurationService.IsEGSVersion = dialog.ConfigIsEGSVersion;
                    ConfigurationService.WizardVersionNumber = _wizardVersionNumber;

                    const int EpicGamesPC = 2;
                    if (ConfigurationService.GameEdition == EpicGamesPC &&
                        Directory.Exists(ConfigurationService.PcReleaseLocation))
                    {
                        File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt"),
                            new string[]
                            {
                                $"mod_path={ConfigurationService.GameModPath}",
                                $"show_console={false}",
                            });
                    }
                    if (ConfigurationService.GameEdition == 2)
                    {
                        PC = true;
                        PanaceaInstalled = ConfigurationService.PanaceaInstalled;
                    }
                    else
                        PC = false;
                }
            });

            OpenLinkCommand = new RelayCommand(url => Process.Start(new ProcessStartInfo(url as string)
            {
                UseShellExecute = true
            }));

            _pcsx2Injector = new Pcsx2Injector(new OperationDispatcher());
            FetchUpdates();

            if (ConfigurationService.WizardVersionNumber < _wizardVersionNumber)
            {
                ///---Everything after this to be removed at later date
                if (ConfigurationService.PcReleaseLocation != null | ConfigurationService.Pcsx2Location != null)
                {
                    var path = ConfigurationService.GameDataLocation;
                    var newpath = Path.Combine(path, "kh2");

                    if (Directory.Exists("data") == false)
                    {
                        MessageBox.Show("Cannot find valid KH2 Extraction" +
                        "\nPlease complete the setup wizard to update panacea and re-extract your game files.", "Thanks for Updating!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else if (Directory.Exists("data") == true)
                    {
                        MessageBox.Show("Thanks for updating the mod manager! Be sure to complete the setup wizard again in order to update your panacea installation.", "Thanks for Updating!", MessageBoxButton.OK);
                    }

                    //If user has "kh2" folder made already in their openkh folder, rename temporarily to complete further steps
                    if (Directory.Exists("kh2") == true)
                    {
                        Directory.Move("kh2", "kh2-namechange-temp");
                    }

                    //From Shan - Move extracted files to sub kh2 folder in location they were installed in
                    if (!Directory.Exists(newpath))
                    {
                        if (Directory.Exists(path) && !Directory.Exists(newpath))
                        {
                            Directory.CreateDirectory(Path.Combine(newpath));
                            Directory.Move(Path.Combine(path, "anm"), Path.Combine(newpath, "anm"));
                            Directory.Move(Path.Combine(path, "ard"), Path.Combine(newpath, "ard"));
                            Directory.Move(Path.Combine(path, "bgm"), Path.Combine(newpath, "bgm"));
                            Directory.Move(Path.Combine(path, "dbg"), Path.Combine(newpath, "dbg"));
                            Directory.Move(Path.Combine(path, "effect"), Path.Combine(newpath, "effect"));
                            Directory.Move(Path.Combine(path, "event"), Path.Combine(newpath, "event"));
                            Directory.Move(Path.Combine(path, "field2d"), Path.Combine(newpath, "field2d"));
                            Directory.Move(Path.Combine(path, "file"), Path.Combine(newpath, "file"));
                            Directory.Move(Path.Combine(path, "gumibattle"), Path.Combine(newpath, "gumibattle"));
                            Directory.Move(Path.Combine(path, "gumiblock"), Path.Combine(newpath, "gumiblock"));
                            Directory.Move(Path.Combine(path, "gumimenu"), Path.Combine(newpath, "gumimenu"));
                            try
                            {
                                Directory.Move(Path.Combine(path, "ICON"), Path.Combine(newpath, "ICON"));
                                Directory.Move(Path.Combine(path, "remastered"), Path.Combine(newpath, "remastered"));
                                Directory.Move(Path.Combine(path, "save_image"), Path.Combine(newpath, "save_image"));
                            }
                            catch { };
                            Directory.Move(Path.Combine(path, "itempic"), Path.Combine(newpath, "itempic"));
                            Directory.Move(Path.Combine(path, "libretto"), Path.Combine(newpath, "libretto"));
                            Directory.Move(Path.Combine(path, "limit"), Path.Combine(newpath, "limit"));
                            Directory.Move(Path.Combine(path, "magic"), Path.Combine(newpath, "magic"));
                            Directory.Move(Path.Combine(path, "map"), Path.Combine(newpath, "map"));
                            Directory.Move(Path.Combine(path, "menu"), Path.Combine(newpath, "menu"));
                            Directory.Move(Path.Combine(path, "minigame"), Path.Combine(newpath, "minigame"));
                            Directory.Move(Path.Combine(path, "msg"), Path.Combine(newpath, "msg"));
                            Directory.Move(Path.Combine(path, "msn"), Path.Combine(newpath, "msn"));
                            Directory.Move(Path.Combine(path, "npack"), Path.Combine(newpath, "npack"));
                            Directory.Move(Path.Combine(path, "obj"), Path.Combine(newpath, "obj"));
                            Directory.Move(Path.Combine(path, "se"), Path.Combine(newpath, "se"));
                            Directory.Move(Path.Combine(path, "vagstream"), Path.Combine(newpath, "vagstream"));
                            Directory.Move(Path.Combine(path, "voice"), Path.Combine(newpath, "voice"));
                            List<string> files = new List<string>
                        {
                            Path.Combine(path,"00areainfo.bin"),
                            Path.Combine(path,"00battle.bin"),
                            Path.Combine(path,"00common.bdx"),
                            Path.Combine(path,"00effect.bar"),
                            Path.Combine(path,"00font.bar"),
                            Path.Combine(path,"00fontimg.bar"),
                            Path.Combine(path,"00localset.bin"),
                            Path.Combine(path,"00objentry.bin"),
                            Path.Combine(path,"00place.bin"),
                            Path.Combine(path,"00progress.bin"),
                            Path.Combine(path,"00sysrc.bar"),
                            Path.Combine(path,"00system.bin"),
                            Path.Combine(path,"00worldpoint.bin"),
                            Path.Combine(path,"03system.bin"),
                            Path.Combine(path,"07localset.bin"),
                            Path.Combine(path,"10font.bar"),
                            Path.Combine(path,"11fontimg.bar"),
                            Path.Combine(path,"12soundinfo.bar"),
                            Path.Combine(path,"13libretto.bin"),
                            Path.Combine(path,"14mission.bar"),
                            Path.Combine(path,"14mission.bin"),
                            Path.Combine(path,"15jigsaw.bin"),
                            Path.Combine(path,"50gb_effect.bar"),
                            Path.Combine(path,"50gumientry.bin"),
                            Path.Combine(path,"50wmentry.bin"),
                            Path.Combine(path,"50worldmap.bin"),
                            Path.Combine(path,"60gb_effect.bar"),
                            Path.Combine(path,"70landing.bar"),
                            Path.Combine(path,"99motion.dbg"),
                            Path.Combine(path,"backup_icon0.png"),
                            Path.Combine(path,"dk_mask.tm2"),
                            Path.Combine(path,"eventviewer.bar"),
                            Path.Combine(path,"GHelpText.bin"),
                            Path.Combine(path,"icon0.png"),
                            Path.Combine(path,"icon0_e.png"),
                            Path.Combine(path,"icon0_jp.png"),
                            Path.Combine(path,"icon0_n.png"),
                            Path.Combine(path,"item-011.imd"),
                            Path.Combine(path,"KH2.IDX"),
                            Path.Combine(path,"KHHD2.5.config"),
                            Path.Combine(path,"KHHD2.8.config"),
                            Path.Combine(path,"libretto-al.bar"),
                            Path.Combine(path,"libretto-bb.bar"),
                            Path.Combine(path,"libretto-ca.bar"),
                            Path.Combine(path,"libretto-dc.bar"),
                            Path.Combine(path,"libretto-di.bar"),
                            Path.Combine(path,"libretto-eh.bar"),
                            Path.Combine(path,"libretto-es.bar"),
                            Path.Combine(path,"libretto-hb.bar"),
                            Path.Combine(path,"libretto-he.bar"),
                            Path.Combine(path,"libretto-lk.bar"),
                            Path.Combine(path,"libretto-lm.bar"),
                            Path.Combine(path,"libretto-mu.bar"),
                            Path.Combine(path,"libretto-nm.bar"),
                            Path.Combine(path,"libretto-po.bar"),
                            Path.Combine(path,"libretto-tr.bar"),
                            Path.Combine(path,"libretto-tt.bar"),
                            Path.Combine(path,"libretto-wi.bar"),
                            Path.Combine(path,"libretto-wm.bar"),
                            Path.Combine(path,"MHelpText.bin"),
                            Path.Combine(path,"radar.tm2"),
                            Path.Combine(path,"THelpText.bin"),
                            Path.Combine(path,"000al.idx"),
                            Path.Combine(path,"000bb.idx"),
                            Path.Combine(path,"000ca.idx"),
                            Path.Combine(path,"000dc.idx"),
                            Path.Combine(path,"000di.idx"),
                            Path.Combine(path,"000eh.idx"),
                            Path.Combine(path,"000es.idx"),
                            Path.Combine(path,"000gumi.idx"),
                            Path.Combine(path,"000hb.idx"),
                            Path.Combine(path,"000he.idx"),
                            Path.Combine(path,"000lk.idx"),
                            Path.Combine(path,"000lm.idx"),
                            Path.Combine(path,"000mu.idx"),
                            Path.Combine(path,"000nm.idx"),
                            Path.Combine(path,"000po.idx"),
                            Path.Combine(path,"000tr.idx"),
                            Path.Combine(path,"000tt.idx"),
                            Path.Combine(path,"000wi.idx"),
                            Path.Combine(path,"000wm.idx"),

                        };
                            foreach (string s in files)
                            {
                                if (File.Exists(s))
                                    File.Move(s, Path.Combine(newpath, Path.GetFileName(s)));
                            }

                        }

                    }

                    //Rename mod folder to kh2, create new empty mod folder, place kh2 folder into mod folder (Fix users 'mod' directory)
                    if (Directory.Exists("kh2") == false)
                    {
                        if (Directory.Exists("mod") == true && Directory.Exists("mod\\kh2") == false)
                        {
                            Directory.Move("mod", "kh2");
                            Directory.CreateDirectory("mod");
                            Directory.Move("kh2", "mod\\kh2");
                        }
                    }
                    //Clean out new mods folder except for users current mods, Rename mods folder to kh2, create new empty mods folder, place kh2 folder into mods folder (Fix users 'mods' directory)
                    if (Directory.EnumerateDirectories("mods\\bbs").Any() == false)
                        Directory.Delete("mods\\bbs");
                    if (Directory.EnumerateDirectories("mods\\kh1").Any() == false)
                        Directory.Delete("mods\\kh1");
                    if (Directory.EnumerateDirectories("mods\\kh2").Any() == false)
                        Directory.Delete("mods\\kh2");
                    if (Directory.EnumerateDirectories("mods\\Recom").Any() == false)
                        Directory.Delete("mods\\Recom");

                    if (Directory.Exists("kh2") == false)
                    {
                        if (Directory.Exists("mods") == true && Directory.Exists("mods\\kh2") == false)
                        {
                            Directory.Move("mods", "kh2");
                            Directory.CreateDirectory("mods");
                            Directory.Move("kh2", "mods\\kh2");
                        }
                    }

                    if (Directory.Exists("mods\\bbs") == false)
                    {
                        Directory.CreateDirectory("mods\\bbs");
                    }
                    if (Directory.Exists("mods\\kh1") == false)
                    {
                        Directory.CreateDirectory("mods\\kh1");
                    }
                    if (Directory.Exists("mods\\Recom") == false)
                    {
                        Directory.CreateDirectory("mods\\Recom");
                    }
                    //Rename mods.txt to mods-KH2.txt
                    if (File.Exists("mods.txt") == true)
                    {
                        if (Directory.Exists("mods-KH2.txt") == false)
                        {
                            File.Move("mods.txt", "mods-KH2.txt");
                        }
                    }
                    // Delete(Replaces) Luabackend.toml from game install folder, place new Luabackend.toml file with updated scripts locations (Fix users luabackend.toml scripts location)
                    if (ConfigurationService.PcReleaseLocation != null & Directory.Exists(ConfigurationService.PcReleaseLocation + "\\Luabackend.toml") == true)
                    {
                        string StoragePath_Fixed = StoragePath;
                        StoragePath_Fixed = StoragePath_Fixed.Replace("\\", "\\\\");
                        string[] lines = { "[kh1]", "scripts = [{ path = \"scripts/kh1/\", relative = true }]", "base = 0x3A0606", "thread_struct = 0x22B7280", "exe = \"KINGDOM HEARTS FINAL MIX.exe\"", "game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", "\n[kh2]", "scripts = [{ path = \"scripts/kh2/\", relative = true }, {path = \"" + StoragePath_Fixed + "\\\\mod\\\\kh2\\\\scripts\",relative = false}]", "base = 0x56454E", "thread_struct = 0x89E9A0", "exe = \"KINGDOM HEARTS II FINAL MIX.exe\"", "game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", "\n[bbs]", "scripts = [{ path = \"scripts/bbs/\", relative = true }]", "base = 0x60E334", "thread_struct = 0x110B5970", "exe = \"KINGDOM HEARTS Birth by Sleep FINAL MIX.exe\"", "game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", "\n[recom]", "scripts = [{ path = \"scripts/recom/\", relative = true }]", "base = 0x4E4660", "thread_struct = 0xBF7A80", "exe = \"KINGDOM HEARTS Re_Chain of Memories.exe\"", "game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", "\n[kh3d]", "scripts = [{ path = \"scripts/kh3d/\", relative = true }]", "base = 0x770E4A", "thread_struct = 0x14DA6F20", "exe = \"KINGDOM HEARTS Dream Drop Distance.exe\"", "game_docs = \"KINGDOM HEARTS HD 2.8 Final Chapter Prologue\"" };
                        using StreamWriter file = new(ConfigurationService.PcReleaseLocation + "\\Luabackend.toml");
                        foreach (string line in lines)
                        {
                            file.WriteLineAsync(line);
                        }
                    }
                    if (Directory.Exists("kh2-namechange-temp") == true)
                    {
                        Directory.Move("kh2-namechange-temp", "kh2");
                    }
                    try
                    {
                        ReloadModsList();
                    }
                    catch (Exception e) { MessageBox.Show("Mod Manager failed to reload mods list! Contact support in the KH2 Randomizer Discord for help", "Error!", MessageBoxButton.OK); }
                }
                WizardCommand.Execute(null);
            }
        }

        public void CloseAllWindows()
        {
            CloseRunningProcess();
            Application.Current.Dispatcher.Invoke(_debuggingWindow.Close);
        }

        public void CloseRunningProcess()
        {
            if (_runningProcess == null)
                return;

            _pcsx2Injector.Stop();
            _runningProcess.CloseMainWindow();
            _runningProcess.Kill();
            _runningProcess.Dispose();
            _runningProcess = null;
            OnPropertyChanged(nameof(StopRunningInstanceCommand));
        }

        private void ResetLogWindow()
        {
            if (_debuggingWindow != null)
                Application.Current.Dispatcher.Invoke(_debuggingWindow.Close);
            _debuggingWindow = new DebuggingWindow();
            Application.Current.Dispatcher.Invoke(_debuggingWindow.Show);
            _debuggingWindow.ClearLogs();
        }

        private async Task<bool> BuildPatches(bool fastMode)
        {
            IsBuilding = true;
            var result = await ModsService.RunPacherAsync(fastMode);
            IsBuilding = false;

            return result;
        }

        private Task RunGame()
        {
            ProcessStartInfo processStartInfo;
            bool isPcsx2 = false;
            switch (ConfigurationService.GameEdition)
            {
                case 0:
                    Log.Info("Starting OpenKH Game Engine");
                    processStartInfo = new ProcessStartInfo
                    {
                        FileName = ConfigurationService.OpenKhGameEngineLocation,
                        WorkingDirectory = Path.GetDirectoryName(ConfigurationService.OpenKhGameEngineLocation),
                        Arguments = $"--data \"{ConfigurationService.GameDataLocation}\" --modpath \"{ConfigurationService.GameModPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                    };
                    break;
                case 1:
                    Log.Info("Starting PCSX2");
                    _pcsx2Injector.RegionId = ConfigurationService.RegionId;
                    _pcsx2Injector.Region = Kh2.Constants.Regions[_pcsx2Injector.RegionId];
                    _pcsx2Injector.Language = Kh2.Constants.Languages[_pcsx2Injector.RegionId];

                    processStartInfo = new ProcessStartInfo
                    {
                        FileName = ConfigurationService.Pcsx2Location,
                        WorkingDirectory = Path.GetDirectoryName(ConfigurationService.Pcsx2Location),
                        Arguments = $"\"{ConfigurationService.IsoLocation}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                    };
                    isPcsx2 = true;
                    break;
                case 2:
                    if (ConfigurationService.IsEGSVersion)
                    {
                        if (ConfigurationService.PanaceaInstalled)
                        {
                            File.AppendAllText(Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt"), "\nquick_launch=" + _launchGame);
                        }                        
                        processStartInfo = new ProcessStartInfo
                        {
                            FileName = "com.epicgames.launcher://apps/4158b699dd70447a981fee752d970a3e%3A5aac304f0e8948268ddfd404334dbdc7%3A68c214c58f694ae88c2dab6f209b43e4?action=launch&silent=true",
                            UseShellExecute = true,
                        };
                    }
                    else
                    {
                        processStartInfo = new ProcessStartInfo
                        {
                            FileName =  Path.Combine(ConfigurationService.PcReleaseLocation, executable[launchExecutable]),
                            WorkingDirectory = ConfigurationService.PcReleaseLocation,
                            UseShellExecute = false,
                        };
                        if (processStartInfo == null || !File.Exists(processStartInfo.FileName))
                        {
                            MessageBox.Show(
                                "Unable to start game. Please make sure your Kingdom Hearts executable is correctly named and in the correct folder.",
                                "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                    }
                    Process.Start(processStartInfo);
                    CloseAllWindows();
                    return Task.CompletedTask;
                default:
                    return Task.CompletedTask;
            }

            if (processStartInfo == null || !File.Exists(processStartInfo.FileName))
            {
                MessageBox.Show(
                    "Unable to locate the executable. Please run the Wizard by going to the Settings menu.",
                    "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                CloseAllWindows();
                return Task.CompletedTask;
            }

            _runningProcess = new Process() { StartInfo = processStartInfo };
            _runningProcess.OutputDataReceived += (sender, e) => CaptureLog(e.Data);
            _runningProcess.ErrorDataReceived += (sender, e) => CaptureLog(e.Data);
            _runningProcess.Start();
            _runningProcess.BeginOutputReadLine();
            _runningProcess.BeginErrorReadLine();
            if (isPcsx2)
                _pcsx2Injector.Run(_runningProcess, _debuggingWindow);

            OnPropertyChanged(nameof(StopRunningInstanceCommand));
            return Task.Run(() =>
            {
                _runningProcess.WaitForExit();
                CloseAllWindows();
            });
        }

        private void CaptureLog(string data)
        {
            if (data == null)
                return;
            else if (data.Contains("err", StringComparison.InvariantCultureIgnoreCase))
                Log.Err(data);
            else if (data.Contains("wrn", StringComparison.InvariantCultureIgnoreCase))
                Log.Warn(data);
            else if (data.Contains("warn", StringComparison.InvariantCultureIgnoreCase))
                Log.Warn(data);
            else
                Log.Info(data);
        }

        private void ReloadModsList()
        {
            ModsList = new ObservableCollection<ModViewModel>(
                ModsService.GetMods(ModsService.Mods).Select(Map));
            OnPropertyChanged(nameof(ModsList));
        }

        private ModViewModel Map(ModModel mod) => new ModViewModel(mod, this);

        public void ModEnableStateChanged()
        {
            ConfigurationService.EnabledMods = ModsList
                .Where(x => x.Enabled)
                .Select(x => x.Source)
                .ToList();
            OnPropertyChanged(nameof(BuildAndRunCommand));
        }

        private void MoveSelectedModDown()
        {
            var selectedIndex = ModsList.IndexOf(SelectedValue);
            if (selectedIndex < 0)
                return;

            var item = ModsList[selectedIndex];
            ModsList.RemoveAt(selectedIndex);
            ModsList.Insert(++selectedIndex, item);
            SelectedValue = ModsList[selectedIndex];
            ModEnableStateChanged();
        }

        private void MoveSelectedModUp()
        {
            var selectedIndex = ModsList.IndexOf(SelectedValue);
            if (selectedIndex < 0)
                return;

            var item = ModsList[selectedIndex];
            ModsList.RemoveAt(selectedIndex);
            ModsList.Insert(--selectedIndex, item);
            SelectedValue = ModsList[selectedIndex];
            ModEnableStateChanged();
        }

        private async Task PatchGame(bool fastMode)
        {
            await Task.Run(() =>
            {
                if (ConfigurationService.GameEdition == 2)
                {
                    // Use the package map file to rearrange the files in the structure needed by the patcher
                    var packageMapLocation = Path.Combine(ConfigurationService.GameModPath, _launchGame , "patch-package-map.txt");
                    var packageMap = File
                        .ReadLines(packageMapLocation)
                        .Select(line => line.Split(" $$$$ "))
                        .ToDictionary(array => array[0], array => array[1]);

                    var patchStagingDir = Path.Combine(ConfigurationService.GameModPath, _launchGame, "patch-staging");
                    if (Directory.Exists(patchStagingDir))
                        Directory.Delete(patchStagingDir, true);
                    Directory.CreateDirectory(patchStagingDir);
                    foreach (var entry in packageMap)
                    {
                        var sourceFile = Path.Combine(ConfigurationService.GameModPath, _launchGame, entry.Key);
                        var destFile = Path.Combine(patchStagingDir, entry.Value);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                        File.Move(sourceFile, destFile);
                    }

                    foreach (var directory in Directory.GetDirectories(Path.Combine(ConfigurationService.GameModPath, _launchGame)))
                        if (!"patch-staging".Equals(Path.GetFileName(directory)))
                            Directory.Delete(directory, true);

                    var stagingDirs = Directory.GetDirectories(patchStagingDir).Select(directory => Path.GetFileName(directory)).ToHashSet();

                    string[] specialDirs = Array.Empty<string>();
                    var specialStagingDir = Path.Combine(patchStagingDir, "special");
                    if (Directory.Exists(specialStagingDir))
                        specialDirs = Directory.GetDirectories(specialStagingDir).Select(directory => Path.GetFileName(directory)).ToArray();

                    foreach (var packageName in stagingDirs)
                        Directory.Move(Path.Combine(patchStagingDir, packageName), Path.Combine(ConfigurationService.GameModPath, _launchGame, packageName));
                    foreach (var specialDir in specialDirs)
                        Directory.Move(Path.Combine(ConfigurationService.GameModPath, _launchGame, "special", specialDir), Path.Combine(ConfigurationService.GameModPath, _launchGame, specialDir));

                    stagingDirs.Remove("special"); // Since it's not actually a real game package
                    Directory.Delete(patchStagingDir, true);

                    var specialModDir = Path.Combine(ConfigurationService.GameModPath, _launchGame, "special");
                    if (Directory.Exists(specialModDir))
                        Directory.Delete(specialModDir, true);

                    foreach (var directory in stagingDirs.Select(packageDir => Path.Combine(ConfigurationService.GameModPath, _launchGame, packageDir)))
                    {
                        if (specialDirs.Contains(Path.GetDirectoryName(directory)))
                            continue;

                        var patchFiles = new List<string>();
                        var _dirPart = new DirectoryInfo(directory).Name;

                        var _orgPath = Path.Combine(directory, ORIGINAL_FILES_FOLDER_NAME);
                        var _rawPath = Path.Combine(directory, RAW_FILES_FOLDER_NAME);

                        if (Directory.Exists(_orgPath))
                            patchFiles = OpenKh.Egs.Helpers.GetAllFiles(_orgPath).ToList();

                        if (Directory.Exists(_rawPath))
                            patchFiles.AddRange(OpenKh.Egs.Helpers.GetAllFiles(_rawPath).ToList());

                        string _pkgSoft;
                        switch (_launchGame)
                        {
                            case "kh1":
                                _pkgSoft = fastMode ? "kh1_first" : _dirPart;
                                break;
                            case "bbs":
                                _pkgSoft = fastMode ? "bbs_first" : _dirPart;
                                break;
                            case "Recom":
                                _pkgSoft = "Recom";
                                break;
                            default:
                                _pkgSoft = fastMode ? "kh2_first" : _dirPart;
                                break;

                        }
                        var _pkgName = Path.Combine(ConfigurationService.PcReleaseLocation, "Image", ConfigurationService.PcReleaseLanguage, _pkgSoft + ".pkg");

                        var _backupDir = Path.Combine(ConfigurationService.PcReleaseLocation, "BackupImage");

                        if (!Directory.Exists(Path.Combine(ConfigurationService.PcReleaseLocation, "BackupImage")))
                            Directory.CreateDirectory(_backupDir);

                        var outputDir = "patchedpkgs";
                        var hedFile = Path.ChangeExtension(_pkgName, "hed");

                        if (!File.Exists(_backupDir + "/" + _pkgSoft + ".pkg"))
                        {
                            Log.Info($"Backing Up Package File {_pkgSoft}");

                            File.Copy(_pkgName, _backupDir + "/" + _pkgSoft + ".pkg");
                            File.Copy(hedFile, _backupDir + "/" + _pkgSoft + ".hed");
                        }

                        else
                        {
                            Log.Info($"Restoring Package File {_pkgSoft}");

                            File.Delete(hedFile);
                            File.Delete(_pkgName);

                            File.Copy(_backupDir + "/" + _pkgSoft + ".pkg", _pkgName);
                            File.Copy(_backupDir + "/" + _pkgSoft + ".hed", hedFile);
                        }

                        using var hedStream = File.OpenRead(hedFile);
                        using var pkgStream = File.OpenRead(_pkgName);
                        var hedHeaders = OpenKh.Egs.Hed.Read(hedStream).ToList();

                        if (!Directory.Exists(outputDir))
                            Directory.CreateDirectory(outputDir);

                        using var patchedHedStream = File.Create(Path.Combine(outputDir, Path.GetFileName(hedFile)));
                        using var patchedPkgStream = File.Create(Path.Combine(outputDir, Path.GetFileName(_pkgName)));

                        foreach (var hedHeader in hedHeaders)
                        {
                            var hash = OpenKh.Egs.Helpers.ToString(hedHeader.MD5);

                            // We don't know this filename, we ignore it
                            if (!OpenKh.Egs.EgsTools.Names.TryGetValue(hash, out var filename))
                                continue;

                            var asset = new OpenKh.Egs.EgsHdAsset(pkgStream.SetPosition(hedHeader.Offset));

                            if (patchFiles.Contains(filename))
                            {
                                patchFiles.Remove(filename);

                                if (hedHeader.DataLength > 0)
                                {
                                    OpenKh.Egs.EgsTools.ReplaceFile(directory, filename, patchedHedStream, patchedPkgStream, asset, hedHeader);
                                    Log.Info($"Replacing File {filename} in {_pkgSoft}");
                                }
                            }

                            else
                            {
                                OpenKh.Egs.EgsTools.ReplaceFile(directory, filename, patchedHedStream, patchedPkgStream, asset, hedHeader);
                                Log.Info($"Skipped File {filename} in {_pkgSoft}");
                            }
                        }

                        // Add all files that are not in the original HED file and inject them in the PKG stream too
                        foreach (var filename in patchFiles)
                        {
                            OpenKh.Egs.EgsTools.AddFile(directory, filename, patchedHedStream, patchedPkgStream);
                            Log.Info($"Adding File {filename} to {_pkgSoft}");
                        }

                        hedStream.Close();
                        pkgStream.Close();

                        patchedHedStream.Close();
                        patchedPkgStream.Close();

                        File.Delete(hedFile);
                        File.Delete(_pkgName);
                        
                        File.Move(Path.Combine(outputDir, Path.GetFileName(hedFile)), hedFile);
                        File.Move(Path.Combine(outputDir, Path.GetFileName(_pkgName)), _pkgName);
                    }
                }
            });
        }

        private async Task RestoreGame(bool patched)
        {
            await Task.Run(() =>
            {
                if (ConfigurationService.GameEdition == 2)
                {                        
                    if(patched)
                    {
                        if (!Directory.Exists(Path.Combine(ConfigurationService.PcReleaseLocation, "BackupImage")))
                        {
                            Log.Warn("backup folder cannot be found! Cannot restore the game.");
                        }
                        else
                        {
                            foreach (var file in Directory.GetFiles(Path.Combine(ConfigurationService.PcReleaseLocation, "BackupImage")).Where(x => x.Contains(".pkg") && (x.Contains(_launchGame))))
                            {
                                Log.Info($"Restoring Package File {file.Replace(".pkg", "")}");

                                var _fileBare = Path.GetFileName(file);
                                var _trueName = Path.Combine(ConfigurationService.PcReleaseLocation, "Image", ConfigurationService.PcReleaseLanguage, _fileBare);

                                File.Delete(Path.ChangeExtension(_trueName, "hed"));
                                File.Delete(_trueName);

                                File.Copy(file, _trueName);
                                File.Copy(Path.ChangeExtension(file, "hed"), Path.ChangeExtension(_trueName, "hed"));
                            }
                        }

                    }
                    if (Directory.Exists(ConfigurationService.GameModPath))
                    {
                        try
                        {
                            Directory.Delete(Path.Combine(ConfigurationService.GameModPath, _launchGame), true);
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("Unable to fully clean the mod directory:\n{0}", ex.Message);
                        }
                    }
                }
            });
        }

        private bool CanSelectedModMoveDown() =>
            SelectedValue != null && ModsList.IndexOf(SelectedValue) < ModsList.Count - 1;

        private bool CanSelectedModMoveUp() =>
            SelectedValue != null && ModsList.IndexOf(SelectedValue) > 0;

        private async Task FetchUpdates()
        {
            await Task.Delay(50); // fixes a bug where the UI wanted to refresh too soon
            await foreach (var modUpdate in ModsService.FetchUpdates())
            {
                var mod = ModsList.FirstOrDefault(x => x.Source == modUpdate.Name);
                if (mod == null)
                    continue;

                Application.Current.Dispatcher.Invoke(() =>
                    mod.UpdateCount = modUpdate.UpdateCount);
            }
        }
    }
}
