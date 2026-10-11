using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] songs;
    public AudioSource player;
    
    private int lastSong = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlaySong();
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.isPlaying)
        {
            PlayNextSong();
        }
    }

    void PlaySong()
    {
        lastSong = Random.Range(0, songs.Length);
        player.clip = songs[lastSong];
        player.Play();
    }

    void PlayNextSong()
    {
        lastSong = (lastSong + 1) % songs.Length;
        player.clip = songs[lastSong];
        player.Play();
    }
}
