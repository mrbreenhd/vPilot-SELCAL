using System;
using System.IO;
using System.Threading;
using NAudio.Wave;

using RossCarlson.Vatsim.Vpilot.Plugins;
using RossCarlson.Vatsim.Vpilot.Plugins.Events;

namespace SELCALTone {
    public class Main : IPlugin {

        public static string version = "1.0.0";

        // Init
        private IBroker vPilot;

        public string Name { get; } = "vPilot SELCAL Tone";

        // Public variables
        public string connectedCallsign = null;
        public string connectedSelcal = null;

        // Settings        
        private static WaveStream audio1;
        private static WaveStream audio2;
        private static WaveStream audio3;
        private static WaveStream audio4;
        private static WaveChannel32 first32;
        private static WaveChannel32 second32;
        private static WaveChannel32 third32;
        private static WaveChannel32 fourth32;

        private static MixingWaveProvider32 mixerA;
        private static MixingWaveProvider32 mixerB;

        private static DirectSoundOut dsoA;
        private static DirectSoundOut dsoB;

        /*
         * 
         * Initilise the plugin
         *
        */
        public void Initialize( IBroker broker ) {
            vPilot = broker;
            vPilot.NetworkConnected += onNetworkConnectedHandler;
            vPilot.NetworkDisconnected += onNetworkDisconnectedHandler;
            vPilot.SelcalAlertReceived += onSelcalAlertReceivedHandler;
            sendDebug("Loading Sucessful");
        }

        public void sendTone(String code)
        {
            sendDebug("Sending Tone for " + code);
            string path = Directory.GetCurrentDirectory();
            string soundPath = path + "\\..\\Sounds\\SELCAL\\";

            try
            {
                char[] characters = code.Replace("-", "").ToCharArray();

                char A = characters[0];
                char B = characters[1];
                char C = characters[2];
                char D = characters[3];

                audio1 = new AudioFileReader(soundPath + A + ".wav");
                audio2 = new AudioFileReader(soundPath + B + ".wav");
                audio3 = new AudioFileReader(soundPath + C + ".wav");
                audio4 = new AudioFileReader(soundPath + D + ".wav");

                first32 = new WaveChannel32(audio1);
                second32 = new WaveChannel32(audio2);
                third32 = new WaveChannel32(audio3);
                fourth32 = new WaveChannel32(audio4);

                mixerA = new MixingWaveProvider32();
                mixerA.AddInputStream(first32);
                mixerA.AddInputStream(second32);

                mixerB = new MixingWaveProvider32();
                mixerB.AddInputStream(third32);
                mixerB.AddInputStream(fourth32);

                dsoA = new DirectSoundOut(DirectSoundOut.DSDEVID_DefaultPlayback);
                dsoB = new DirectSoundOut(DirectSoundOut.DSDEVID_DefaultPlayback);

                dsoA.Init(mixerA);
                dsoB.Init(mixerB);

                dsoA.Play();
                Thread.Sleep(1200);
                dsoB.Play();

                sendDebug("Tone played for " + code);
            }
            catch (Exception ex)
            {
                sendDebug(ex.Message);
            }
        }

        public void sendDebug( String text ) {
            vPilot.PostDebugMessage(text);
        }

        private void onNetworkConnectedHandler( object sender, NetworkConnectedEventArgs e ) {
            connectedCallsign = e.Callsign;
            connectedSelcal = e.SelcalCode;
            sendDebug("Connected Callsign: " + connectedCallsign + " And Connected SELCAL: " + connectedSelcal);
        }

        private void onNetworkDisconnectedHandler( object sender, EventArgs e ) {
            connectedCallsign = null;
            connectedSelcal = null;
        }

        private void onSelcalAlertReceivedHandler( object sender, SelcalAlertReceivedEventArgs e ) {
            sendTone(connectedSelcal);
            sendDebug("Received SELCAL Sending Tone");
        }
    }
}