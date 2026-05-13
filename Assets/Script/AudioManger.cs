using UnityEngine;

public class AudioManger : MonoBehaviour
{


public static AudioManger instance;


    public AudioSource musicSource;

    public AudioClip musicClips;
    public AudioClip coinClips;
    
    public AudioClip[] musicClipsArray;
    private void Awake()
    {


        Debug.Log("AudioManger Awake called");
        if (instance == null) 
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


    }

    private void Start()
    {
        Debug.Log("AudioManger Start called");


        PlayMusic();

        foreach( AudioClip clip in musicClipsArray)
        {
            Debug.Log("Music Clip: " + clip.name);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            PlayCoinSound();
        }
    }

    public void PlayMusic()
    {
       
            musicSource.clip = musicClips;
            musicSource.Play();
        
    }
    public void PlayCoinSound()
    {
       
        
            musicSource.PlayOneShot(coinClips);
        
    }


}
