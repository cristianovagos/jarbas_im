using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        private Tts t;

        private string[] months = new string[] { "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro" };

        private bool acceptingVoiceInput = false;

        private bool awaitingVoiceConfirmation = false;
        private bool awaitingVoiceGender = false;
        private bool awaitingConfirmation = false;
        private bool awaitingShutdownConfirmation = false;

        private string callClient = null;

        /*private static Action functionToCall = () => { };
        private System.Threading.Timer timer = new System.Threading.Timer(
            callback => functionToCall(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(2));*/

        public MainWindow()
        {
            InitializeComponent();

            t = new Tts();
            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            
            init();
        }

        private void init()
        {
            if (!Properties.Settings.Default.configured)
                configureJarbas();

            mmiC.Start();
        }

        private void configureJarbas()
        {
            acceptingVoiceInput = false;

            if(Properties.Settings.Default.firstRun)
            {
                t.Speak("Olá! Eu sou o Jarbas, o seu assistente no Windows. " +
                "Como esta é a minha primeira execução, vou-lhe fazer algumas perguntas.");
            }
            else
            {
                t.Speak("Vamos dar início à configuração do Jarbas. " +
                "Como é que deseja ser tratado?");
            }

            askForVoiceConfig();
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            if (!acceptingVoiceInput) return;

            XNamespace emma_ns = "http://www.w3.org/2003/04/emma";
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            string raw_confidence = (string)doc.Descendants(emma_ns+"interpretation").FirstOrDefault().Attribute(emma_ns+"confidence").Value;
            float confidence = float.Parse(raw_confidence);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            onMessage(json, confidence);

            /*App.Current.Dispatcher.Invoke(() =>
            {
                switch ((string)json.recognized[1].ToString())
                {
                    case "GREEN":
                        _s.Fill = Brushes.Green;
                        break;
                    case "BLUE":
                        _s.Fill = Brushes.Blue;
                        break;
                    case "RED":
                        _s.Fill = Brushes.Red;
                        break;
                }
            });*/
        }

        private void onMessage(dynamic json, float confidence)
        {
            string command = (string)json.recognized[0].ToString();

            if ((!command.Equals("not_ok") || !command.Equals("ok")) && awaitingConfirmation)
                return;

            if ((!command.Equals("boss") || !command.Equals("chefe") || !command.Equals("mestre") ||
                !command.Equals("professor") || !command.Equals("engenheiro")) && awaitingVoiceConfirmation)
                return;

            if ((!command.Equals("male") || !command.Equals("female")) && awaitingVoiceGender)
                return;

            switch (command)
            {
                case "lock":
                    LockWorkStation();
                    break;

                case "desligar":
                    askForShutdownConfirmation();
                    break;

                case "horas":
                    sayHour();
                    break;

                case "data_dia":
                case "data_mes":
                case "data_ano":
                    sayDate(command);
                    break;

                case "calc":
                    Process.Start("calc.exe");
                    break;

                case "meteo":
                    Process.Start(@"msnweather://forecast?la=40.6442700&lo=-8.6455400");
                    break;

                case "email":
                    Process.Start("mailto:cristianovagos@ua.pt,gpatricio@ua.pt?Subject=O Jarbas envia mails&Body=Queremos ter boa nota, professor!");
                    break;

                case "website":
                    Process.Start("www.ua.pt");
                    break;

                case "iniciar":
                    startMenuToggle();
                    break;

                case "bateria":
                    sayBatteryInfo();
                    break;

                case "config":
                    configureJarbas();
                    break;

                case "male":
                case "female":
                    onVoiceGenderConfigReceived(command);
                    break;

                case "boss":
                case "chefe":
                case "mestre":
                case "professor":
                case "engenheiro":
                    callClient = command;
                    onVoiceConfigReceived();
                    break;

                case "not_ok":
                    if (!awaitingConfirmation)
                        return;
                    else if (awaitingVoiceConfirmation)
                        askForVoiceConfig();
                    else if (awaitingShutdownConfirmation)
                        awaitingShutdownConfirmation = false;

                    break;

                case "ok":
                    if (!awaitingConfirmation)
                        return;
                    else if (awaitingVoiceConfirmation)
                        onVoiceConfigConfirmation();
                    else if (awaitingShutdownConfirmation && confidence > 0.9F)
                        shutdownPC();

                    break;

                case "cancel":
                    awaitingConfirmation = false;
                    awaitingVoiceConfirmation = false;
                    awaitingVoiceGender = false;
                    awaitingShutdownConfirmation = false;
                    break;

                default:
                    t.Speak("Não entendi, pode repetir se faz favor?");
                    break;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        private void shutdownPC()
        {
            t.Speak("Adeus " + Properties.Settings.Default.voiceCallClient + ", até à próxima!");

            awaitingShutdownConfirmation = false;
            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        private void askForShutdownConfirmation()
        {
            acceptingVoiceInput = false;

            t.Speak(Properties.Settings.Default.voiceCallClient + ", tem a certeza que pretende desligar o computador?");

            acceptingVoiceInput = true;
            awaitingConfirmation = true;
            awaitingShutdownConfirmation = true;
        }

        private void sayHour()
        {
            t.Speak(Properties.Settings.Default.voiceCallClient + ", são " + DateTime.Now.Hour + " horas e " + DateTime.Now.Minute + 
                " minutos.");
        }

        private void sayDate(string command)
        {
            string phrase = Properties.Settings.Default.voiceCallClient;

            switch (command)
            {
                case "data_dia":
                    phrase += " , hoje é dia " + DateTime.Now.Day;
                    break;
                case "data_mes":
                    phrase += " , estamos no mês " + months[DateTime.Now.Month-1];
                    break;
                case "data_ano":
                    phrase += " , estamos no ano " + DateTime.Now.Year;
                    break;
            }
            t.Speak(phrase);
        }

        [DllImport("User32")]
        private static extern int keybd_event(Byte bVk, Byte bScan, long dwFlags, long dwExtraInfo);
        private const byte UP = 2;
        private const byte CTRL = 17;
        private const byte ESC = 27;

        private void startMenuToggle()
        {
            t.Speak(Properties.Settings.Default.voiceCallClient + ", aqui está o menu de Start do Windows");

            // Press Ctrl-Esc key to open Start menu
            keybd_event(CTRL, 0, 0, 0);
            keybd_event(ESC, 0, 0, 0);

            // Need to Release those two keys
            keybd_event(CTRL, 0, UP, 0);
            keybd_event(ESC, 0, UP, 0);
        }

        private void sayBatteryInfo()
        {
            PowerStatus p = SystemInformation.PowerStatus;
            int a = (int)(p.BatteryLifePercent * 100);
            if (a < 20)
                t.Speak(Properties.Settings.Default.voiceCallClient + ", tem neste momento " + a + "% de bateria. " +
                    "Se quer a minha opinião, penso que é boa ideia ligar o seu computador à corrente.");
            else
                t.Speak(Properties.Settings.Default.voiceCallClient + ", tem neste momento " + a + "% de bateria.");
        }

        private void askForVoiceConfig()
        {
            acceptingVoiceInput = false;

            t.Speak("Como é que deseja ser tratado?");

            acceptingVoiceInput = true;
            awaitingVoiceConfirmation = true;
        }

        private void onVoiceConfigReceived()
        {
            acceptingVoiceInput = false;

            t.Speak("OK " + callClient + ", a partir de agora irei tratá-lo assim. " +
                "Tem a certeza que quer este tipo de tratamento?");

            acceptingVoiceInput = true;
            awaitingConfirmation = true;
        }

        private void onVoiceConfigConfirmation()
        {
            acceptingVoiceInput = false;

            t.Speak("OK " + callClient + ", configuração guardada. ");

            awaitingVoiceConfirmation = false;
            Properties.Settings.Default.voiceCallClient = callClient;
            Properties.Settings.Default.Save();

            askForVoiceGenderConfig();
        }

        private void askForVoiceGenderConfig()
        {
            acceptingVoiceInput = false;

            t.Speak(Properties.Settings.Default.voiceCallClient + ", que género prefere para a minha voz?");

            acceptingVoiceInput = true;
            awaitingVoiceGender = true;
        }

        private void onVoiceGenderConfigReceived(string gender)
        {
            if (gender.Equals("male"))
                t.changeVoiceGender(true);
            else
                t.changeVoiceGender(false);

            t.Speak("Género da voz seleccionado com sucesso.");

            Properties.Settings.Default.voiceGender = gender;
            Properties.Settings.Default.Save();

            awaitingVoiceGender = false;

            onVoiceGenderConfigCompleted();
        }

        private void onVoiceGenderConfigCompleted()
        {
            t.Speak("Muito bem " + callClient + ", a configuração do assistente está completa.");

            Properties.Settings.Default.configured = true;
            Properties.Settings.Default.firstRun = false;
            Properties.Settings.Default.Save();
        }
    }
}
