using Android.Content;
using Android.Media;
using repeat_timer_maui.Services;

namespace repeat_timer_maui.Platforms.Android.Services
{
    public sealed class AndroidAlarmPlayer : IAlarmPlayer
    {
        private readonly Context _context;
        private Ringtone? _ringtone;

        public AndroidAlarmPlayer()
        {
            _context = global::Android.App.Application.Context;
        }

        public void Start()
        {
            Stop();

            var alarmUri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm)
                ?? RingtoneManager.GetDefaultUri(RingtoneType.Notification)
                ?? RingtoneManager.GetDefaultUri(RingtoneType.Ringtone);

            if (alarmUri is null)
            {
                return;
            }

            _ringtone = RingtoneManager.GetRingtone(_context, alarmUri);

            if (_ringtone is null)
            {
                return;
            }

            _ringtone.Looping = true;
            _ringtone.Play();
        }

        public void Stop()
        {
            if (_ringtone is null)
            {
                return;
            }

            if (_ringtone.IsPlaying)
            {
                _ringtone.Stop();
            }

            _ringtone.Dispose();
            _ringtone = null;
        }
    }
}