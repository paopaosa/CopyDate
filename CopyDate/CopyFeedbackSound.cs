using System.Media;

namespace CopyDate;

internal static class CopyFeedbackSound
{
    private static readonly Lazy<SoundPlayer?> Player = new(Load);

    public static void Play()
    {
        try
        {
            Player.Value?.Play();
        }
        catch (Exception ex) when (ex is InvalidOperationException or TimeoutException or IOException)
        {
            // Feedback only; never block copy on sound failure.
        }
    }

    private static SoundPlayer? Load()
    {
        Stream? stream = typeof(CopyFeedbackSound).Assembly
            .GetManifestResourceStream("CopyDate.Assets.copy-ding.wav");
        if (stream is null)
        {
            return null;
        }

        var player = new SoundPlayer(stream);
        player.Load();
        return player;
    }
}
