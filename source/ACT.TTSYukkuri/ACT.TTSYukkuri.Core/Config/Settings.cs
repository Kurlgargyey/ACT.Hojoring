using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using ACT.TTSYukkuri.Boyomichan;
using ACT.TTSYukkuri.Discord.Models;
using Amazon.Polly;
using FFXIV.Framework.Common;
using FFXIV.Framework.Globalization;
using Prism.Mvvm;

namespace ACT.TTSYukkuri.Config
{
    /// <summary>
    /// TTSYukkuri設定
    /// </summary>
    [Serializable]
    [XmlRoot("TTSYukkuriConfig")]
    [XmlType("TTSYukkuriConfig")]
    public class Settings :
        BindableBase
    {
        #region Singleton

        [XmlIgnore]
        private static object lockObject = new object();

        [XmlIgnore]
        private static Settings instance;

        /// <summary>
        /// シングルトンインスタンスを返す
        /// </summary>
        [XmlIgnore]
        public static Settings Default
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new Settings();
                        instance.Load();
                    }

                    return instance;
                }
            }
        }

        #endregion Singleton

        /// <summary>
        /// 初期化中か？
        /// </summary>
        [XmlIgnore]
        public static bool IsInitializing { get; set; }

        [XmlIgnore]
        public const double UpdateCheckInterval = 12;

        /// <summary>
        /// 設定ファイルのパスを返す
        /// </summary>
        [XmlIgnore]
        public static string FilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "anoyetta\\ACT\\ACT.TTSYukkuri.config");

        private DateTime lastUpdateDateTime = DateTime.Now;
        private string tts = TTSType.Yukkuri;
        private bool waveCacheClearEnable;
        private int waveVolume = 100;
        private WavePlayerTypes player = WavePlayerTypes.WASAPIBuffered;
        private bool isSyncPlayback = false;
        private string mainDeviceID;
        private bool enabledSubDevice;
        private string subDeviceID;
        private string boyomiServer = BoyomichanSpeechController.BoyomichanServer;
        private int boyomiPort = BoyomichanSpeechController.BoyomichanServicePort;
        private YukkuriConfig yukkuriSettings = new YukkuriConfig();
        private YukkuriConfig yukkuriSettingsExt1 = new YukkuriConfig();
        private YukkuriConfig yukkuriSettingsExt2 = new YukkuriConfig();
        private YukkuriConfig yukkuriSettingsExt3 = new YukkuriConfig();
        private HOYAConfig hoyaSettings = new HOYAConfig();
        private HOYAConfig hoyaSettingsExt1 = new HOYAConfig();
        private HOYAConfig hoyaSettingsExt2 = new HOYAConfig();
        private HOYAConfig hoyaSettingsExt3 = new HOYAConfig();
        private PollyConfigs pollySettings = new PollyConfigs();
        private PollyConfigs pollySettingsExt1 = new PollyConfigs();
        private PollyConfigs pollySettingsExt2 = new PollyConfigs();
        private PollyConfigs pollySettingsExt3 = new PollyConfigs();
        private OpenJTalkConfig openJTalkSettings = new OpenJTalkConfig();
        private OpenJTalkConfig openJTalkSettingsExt1 = new OpenJTalkConfig();
        private OpenJTalkConfig openJTalkSettingsExt2 = new OpenJTalkConfig();
        private OpenJTalkConfig openJTalkSettingsExt3 = new OpenJTalkConfig();
        private SasaraConfig sasaraSettings = new SasaraConfig();
        private CevioAIConfig cevioAISettings = new CevioAIConfig();
        private VoiceroidConfig voiceroidSettings = new VoiceroidConfig();
        private SAPI5Configs sapi5Settings = new SAPI5Configs();
        private SAPI5Configs sapi5SettingsExt1 = new SAPI5Configs();
        private SAPI5Configs sapi5SettingsExt2 = new SAPI5Configs();
        private SAPI5Configs sapi5SettingsExt3 = new SAPI5Configs();
        private GoogleCloudTextToSpeechConfig googleCloudTextToSpeechSettings = new GoogleCloudTextToSpeechConfig();
        private GoogleCloudTextToSpeechConfig googleCloudTextToSpeechSettingsExt1 = new GoogleCloudTextToSpeechConfig();
        private GoogleCloudTextToSpeechConfig googleCloudTextToSpeechSettingsExt2 = new GoogleCloudTextToSpeechConfig();
        private GoogleCloudTextToSpeechConfig googleCloudTextToSpeechSettingsExt3 = new GoogleCloudTextToSpeechConfig();
        private StatusAlertConfig statusAlertSettings = new StatusAlertConfig();
        private DiscordSettings discordSettings = new DiscordSettings();

        /// <summary>
        /// プラグインのUIのロケール
        /// </summary>
        [XmlIgnore]
        public Locales UILocale => FFXIV.Framework.Config.Instance.UILocale;

        /// <summary>
        /// FFXIVのロケール
        /// </summary>
        public Locales FFXIVLocale => FFXIV.Framework.Config.Instance.XIVLocale;

        /// <summary>
        /// 最終アップデート日時
        /// </summary>
        [XmlIgnore]
        public DateTime LastUpdateDateTime
        {
            get => this.lastUpdateDateTime;
            set => this.SetProperty(ref this.lastUpdateDateTime, value);
        }

        /// <summary>
        /// 最終アップデート日時
        /// </summary>
        [XmlElement(ElementName = "LastUpdateDateTime")]
        public string LastUpdateDateTimeCrypted
        {
            get => Crypter.EncryptString(this.lastUpdateDateTime.ToString("o"));
            set
            {
                DateTime d;
                if (DateTime.TryParse(value, out d))
                {
                    if (d > DateTime.Now)
                    {
                        d = DateTime.Now;
                    }

                    this.lastUpdateDateTime = d;
                    return;
                }

                try
                {
                    var decrypt = Crypter.DecryptString(value);
                    if (DateTime.TryParse(decrypt, out d))
                    {
                        if (d > DateTime.Now)
                        {
                            d = DateTime.Now;
                        }

                        this.lastUpdateDateTime = d;
                        return;
                    }
                }
                catch (Exception)
                {
                }

                this.lastUpdateDateTime = DateTime.MinValue;
            }
        }

        /// <summary>
        /// TTSの種類
        /// </summary>
        public string TTS
        {
            get => this.tts;
            set => this.SetProperty(ref this.tts, value);
        }

        /// <summary>
        /// 終了時にキャッシュしたwaveファイルを削除する
        /// </summary>
        public bool WaveCacheClearEnable
        {
            get => this.waveCacheClearEnable;
            set => this.SetProperty(ref this.waveCacheClearEnable, value);
        }

        /// <summary>
        /// Wave再生時のボリューム
        /// </summary>
        public int WaveVolume
        {
            get => this.waveVolume;
            set => this.SetProperty(ref this.waveVolume, value);
        }

        /// <summary>
        /// 再生方式
        /// </summary>
        public WavePlayerTypes Player
        {
            get => this.player;
            set
            {
                if (this.SetProperty(ref this.player, value))
                {
                    this.playDevices.Clear();
                    this.playDevices.AddRange(WavePlayer.EnumerateDevices(this.player));

                    this.RaisePropertyChanged(nameof(this.SyncPlaybackVisibility));
                }
            }
        }

        private readonly ObservableCollection<PlayDevice> playDevices = new ObservableCollection<PlayDevice>();

        [XmlIgnore]
        public ObservableCollection<PlayDevice> PlayDevices
        {
            get
            {
                if (this.playDevices.Count <= 0)
                {
                    this.playDevices.AddRange(WavePlayer.EnumerateDevices(this.player));
                }

                return this.playDevices;
            }
        }

        [XmlIgnore]
        public bool SyncPlaybackVisibility => this.Player == WavePlayerTypes.WASAPIBuffered;

        /// <summary>
        /// 同期再生を行うか？
        /// </summary>
        public bool IsSyncPlayback
        {
            get => this.isSyncPlayback;
            set => this.SetProperty(ref this.isSyncPlayback, value);
        }

        private bool isClearBufferAtWipeout = true;

        /// <summary>
        /// Wipeout時にバッファをクリアする
        /// </summary>
        public bool IsClearBufferAtWipeout
        {
            get => this.isClearBufferAtWipeout;
            set => this.SetProperty(ref this.isClearBufferAtWipeout, value);
        }

        /// <summary>
        /// メイン再生デバイスID
        /// </summary>
        public string MainDeviceID
        {
            get => this.mainDeviceID;
            set
            {
                if (this.SetProperty(ref this.mainDeviceID, value))
                {
                    if (!IsInitializing)
                    {
                        (DiscordClientModel.Model as DiscordNetModel)?.ClearQueue();

                        if (this.mainDeviceID != PlayDevice.DiscordDeviceID)
                        {
                            SoundPlayerWrapper.Init();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// サブ再生デバイスを有効にする
        /// </summary>
        public bool EnabledSubDevice
        {
            get => this.enabledSubDevice;
            set => this.SetProperty(ref this.enabledSubDevice, value);
        }

        /// <summary>
        /// サブ再生デバイスID
        /// </summary>
        public string SubDeviceID
        {
            get => this.subDeviceID;
            set
            {
                if (this.SetProperty(ref this.subDeviceID, value))
                {
                    if (!IsInitializing)
                    {
                        (DiscordClientModel.Model as DiscordNetModel)?.ClearQueue();

                        if (this.subDeviceID != PlayDevice.DiscordDeviceID)
                        {
                            SoundPlayerWrapper.Init();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 棒読みサーバ
        /// </summary>
        public string BoyomiServer
        {
            get => this.boyomiServer;
            set => this.SetProperty(ref this.boyomiServer, value);
        }

        /// <summary>
        /// 棒読みサーバのポート
        /// </summary>
        public int BoyomiPort
        {
            get => this.boyomiPort;
            set => this.SetProperty(ref this.boyomiPort, value);
        }

        private bool isBoyomiInterruptNotication;

        public bool IsBoyomiInterruptNotication
        {
            get => this.isBoyomiInterruptNotication;
            set => this.SetProperty(ref this.isBoyomiInterruptNotication, value);
        }

        /// <summary>
        /// AquesTalk(ゆっくり)の設定
        /// </summary>
        public YukkuriConfig YukkuriSettings
        {
            get => this.yukkuriSettings;
            set => this.SetProperty(ref this.yukkuriSettings, value);
        }

        public YukkuriConfig YukkuriSettingsExt1
        {
            get => this.yukkuriSettingsExt1;
            set => this.SetProperty(ref this.yukkuriSettingsExt1, value);
        }

        public YukkuriConfig YukkuriSettingsExt2
        {
            get => this.yukkuriSettingsExt2;
            set => this.SetProperty(ref this.yukkuriSettingsExt2, value);
        }

        public YukkuriConfig YukkuriSettingsExt3
        {
            get => this.yukkuriSettingsExt3;
            set => this.SetProperty(ref this.yukkuriSettingsExt3, value);
        }

        /// <summary>
        /// HOYA VoiceTextWebAPI 設定
        /// </summary>
        public HOYAConfig HOYASettings
        {
            get => this.hoyaSettings;
            set => this.SetProperty(ref this.hoyaSettings, value);
        }

        public HOYAConfig HOYASettingsExt1
        {
            get => this.hoyaSettingsExt1;
            set => this.SetProperty(ref this.hoyaSettingsExt1, value);
        }

        public HOYAConfig HOYASettingsExt2
        {
            get => this.hoyaSettingsExt2;
            set => this.SetProperty(ref this.hoyaSettingsExt2, value);
        }

        public HOYAConfig HOYASettingsExt3
        {
            get => this.hoyaSettingsExt3;
            set => this.SetProperty(ref this.hoyaSettingsExt3, value);
        }

        /// <summary>
        /// Amazon Polly VoiceTextWebAPI 設定
        /// </summary>
        public PollyConfigs PollySettings
        {
            get => this.pollySettings;
            set => this.SetProperty(ref this.pollySettings, value);
        }

        public PollyConfigs PollySettingsExt1
        {
            get => this.pollySettingsExt1;
            set => this.SetProperty(ref this.pollySettingsExt1, value);
        }

        public PollyConfigs PollySettingsExt2
        {
            get => this.pollySettingsExt2;
            set => this.SetProperty(ref this.pollySettingsExt2, value);
        }

        public PollyConfigs PollySettingsExt3
        {
            get => this.pollySettingsExt3;
            set => this.SetProperty(ref this.pollySettingsExt3, value);
        }

        private ObservableCollection<PollyConfigs.PollyVoice> pollyVoices = new ObservableCollection<PollyConfigs.PollyVoice>
        {
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Ivy.Value} (en-US, Female)", Value = VoiceId.Ivy.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Joanna.Value} (en-US, Female)", Value = VoiceId.Joanna.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Joey.Value} (en-US, Male)", Value = VoiceId.Joey.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Justin.Value} (en-US, Male)", Value = VoiceId.Justin.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Kendra.Value} (en-US, Female)", Value = VoiceId.Kendra.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Kimberly.Value} (en-US, Female)", Value = VoiceId.Kimberly.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Matthew.Value} (en-US, Male)", Value = VoiceId.Matthew.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Salli.Value} (en-US, Female)", Value = VoiceId.Salli.Value },

            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Celine.Value} (fr-FR, Female)", Value = VoiceId.Celine.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Mathieu.Value} (fr-FR, Male)", Value = VoiceId.Mathieu.Value },

            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Hans.Value} (de-DE, Male)", Value = VoiceId.Hans.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Marlene.Value} (de-DE, Female)", Value = VoiceId.Marlene.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Vicki.Value} (de-DE, Female)", Value = VoiceId.Vicki.Value },

            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Mizuki.Value} (ja-JP, Female)", Value = VoiceId.Mizuki.Value },
            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Takumi.Value} (ja-JP, Male)", Value = VoiceId.Takumi.Value },

            new PollyConfigs.PollyVoice { Name = $"{VoiceId.Seoyeon.Value} (ko-KR, Female)", Value = VoiceId.Seoyeon.Value },
        };

        public ObservableCollection<PollyConfigs.PollyVoice> PollyVoices
        {
            get => this.pollyVoices;
            set => this.SetProperty(ref this.pollyVoices, value);
        }

        /// <summary>
        /// OpenJTalk設定
        /// </summary>
        public OpenJTalkConfig OpenJTalkSettings
        {
            get => this.openJTalkSettings;
            set => this.SetProperty(ref this.openJTalkSettings, value);
        }

        public OpenJTalkConfig OpenJTalkSettingsExt1
        {
            get => this.openJTalkSettingsExt1;
            set => this.SetProperty(ref this.openJTalkSettingsExt1, value);
        }

        public OpenJTalkConfig OpenJTalkSettingsExt2
        {
            get => this.openJTalkSettingsExt2;
            set => this.SetProperty(ref this.openJTalkSettingsExt2, value);
        }

        public OpenJTalkConfig OpenJTalkSettingsExt3
        {
            get => this.openJTalkSettingsExt3;
            set => this.SetProperty(ref this.openJTalkSettingsExt3, value);
        }

        /// <summary>
        /// ささら設定
        /// </summary>
        public SasaraConfig SasaraSettings
        {
            get => this.sasaraSettings;
            set => this.SetProperty(ref this.sasaraSettings, value);
        }

        /// <summary>
        /// CeVIO AI設定
        /// </summary>
        public CevioAIConfig CevioAISettings
        {
            get => this.cevioAISettings;
            set => this.SetProperty(ref this.cevioAISettings, value);
        }

        /// <summary>
        /// VOICEROID設定
        /// </summary>
        public VoiceroidConfig VoiceroidSettings
        {
            get => this.voiceroidSettings;
            set => this.SetProperty(ref this.voiceroidSettings, value);
        }

        /// <summary>
        /// SAPI5の設定
        /// </summary>
        public SAPI5Configs SAPI5Settings
        {
            get => this.sapi5Settings;
            set => this.SetProperty(ref this.sapi5Settings, value);
        }

        public SAPI5Configs SAPI5SettingsExt1
        {
            get => this.sapi5SettingsExt1;
            set => this.SetProperty(ref this.sapi5SettingsExt1, value);
        }

        public SAPI5Configs SAPI5SettingsExt2
        {
            get => this.sapi5SettingsExt2;
            set => this.SetProperty(ref this.sapi5SettingsExt2, value);
        }

        public SAPI5Configs SAPI5SettingsExt3
        {
            get => this.sapi5SettingsExt3;
            set => this.SetProperty(ref this.sapi5SettingsExt3, value);
        }

        /// <summary>
        /// Google Cloud Text-To-Speechの設定
        /// </summary>
        public GoogleCloudTextToSpeechConfig GoogleCloudTextToSpeechSettings
        {
            get => this.googleCloudTextToSpeechSettings;
            set => this.SetProperty(ref this.googleCloudTextToSpeechSettings, value);
        }

        public GoogleCloudTextToSpeechConfig GoogleCloudTextToSpeechSettingsExt1
        {
            get => this.googleCloudTextToSpeechSettingsExt1;
            set => this.SetProperty(ref this.googleCloudTextToSpeechSettingsExt1, value);
        }

        public GoogleCloudTextToSpeechConfig GoogleCloudTextToSpeechSettingsExt2
        {
            get => this.googleCloudTextToSpeechSettingsExt2;
            set => this.SetProperty(ref this.googleCloudTextToSpeechSettingsExt2, value);
        }

        public GoogleCloudTextToSpeechConfig GoogleCloudTextToSpeechSettingsExt3
        {
            get => this.googleCloudTextToSpeechSettingsExt3;
            set => this.SetProperty(ref this.googleCloudTextToSpeechSettingsExt3, value);
        }

        /// <summary>
        /// オプション設定
        /// </summary>
        public StatusAlertConfig StatusAlertSettings
        {
            get => this.statusAlertSettings;
            set => this.SetProperty(ref this.statusAlertSettings, value);
        }

        /// <summary>
        /// Discordの設定
        /// </summary>
        public DiscordSettings DiscordSettings
        {
            get => this.discordSettings;
            set => this.SetProperty(ref this.discordSettings, value);
        }

        /// <summary>
        /// サウンド再生のインターバル
        /// </summary>
        public double GlobalSoundInterval { get; set; } = 0.4d;

        /// <summary>
        /// 設定をロードする
        /// </summary>
        public void Load()
        {
            lock (lockObject)
            {
                try
                {
                    IsInitializing = true;

                    var file = FilePath;

                    var activeConfig = this;

                    // サイズ0のファイルがもしも存在したら消す
                    if (File.Exists(file))
                    {
                        var fi = new FileInfo(file);
                        if (fi.Length <= 0)
                        {
                            File.Delete(file);
                        }
                    }

                    if (File.Exists(file))
                    {
                        using (var sr = new StreamReader(file, new UTF8Encoding(false)))
                        {
                            var xs = new XmlSerializer(typeof(Settings));
                            instance = (Settings)xs.Deserialize(sr);

                            activeConfig = instance;
                        }
                    }

                    if (activeConfig != null)
                    {
                        // ステータスアラートの対象を初期化する
                        activeConfig.StatusAlertSettings?.SetDefaultAlertTargets();

                        // 棒読みちゃんのサーバーアドレスを置き換える
                        if (activeConfig.BoyomiServer.StartsWith("127.0.0."))
                        {
                            activeConfig.BoyomiServer = "localhost";
                        }

                        activeConfig.SasaraSettings.LoadRemoteConfig();
                        activeConfig.SasaraSettings.IsInitialized = true;

                        activeConfig.CevioAISettings.LoadRemoteConfig();
                        activeConfig.CevioAISettings.IsInitialized = true;
                    }
                }
                finally
                {
                    IsInitializing = false;
                }
            }
        }

        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

        /// <summary>
        /// 設定をセーブする
        /// </summary>
        public void Save()
        {
            lock (lockObject)
            {
                var file = FilePath;

                var dir = Path.GetDirectoryName(file);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);

                using (var sw = new StreamWriter(file, false, DefaultEncoding))
                {
                    var xs = new XmlSerializer(typeof(Settings));
                    xs.Serialize(sw, Default, ns);
                    sw.Close();
                }
            }
        }
    }
}
