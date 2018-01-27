using UnityEngine;

[System.Serializable]
public class sound{

    public string name;
    public AudioClip clip;

    [Range(0f, 2f)]
    public float volume = 0.7f;
    [Range(0.5f, 1.5f)]
    public float pitch = 1f;

    [Range(0f, 0.5f)]
    public float RandomVolume = 0.1f;
    [Range(0f, 0.5f)]
    public float RandomPitch = 0.1f;

    private AudioSource source;

    public void setSource(AudioSource _scource)
    {
        source = _scource;
        source.clip = clip;
    }

    public void play()
    {
        if (source != null)
        {
            source.volume = volume * (1 + Random.Range(-RandomVolume / 2f, RandomVolume / 2f));
            source.pitch = pitch * (1 + Random.Range(-RandomPitch / 2f, RandomPitch / 2f));
            source.Play();
        }
    }

    public void Stop()
    {
        source.Stop();
    }

    public void ChangeVolume(float change)
    {
        source.volume += change;
        if(source.volume < 0)
        {
            source.volume = 0;
        }
    }

    public bool IsPlaying()
    {
        return source.isPlaying;
    }

    public bool HasVolume()
    {
        return source.volume > 0;
    }

    public void setVolume(float volume)
    {
        if(source != null)
            source.volume = volume;
    }

    public void SetSpatialBlend(float sBlend)
    {
        source.spatialBlend = sBlend;
    }
}

public class AudioManager : MonoBehaviour {

    public static AudioManager instance;

    [SerializeField]
    sound[] sounds;

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogError("More than one AudioManager in the scene");
        }
        else
        {
            instance = this;
        }

        for (int i = 0; i < sounds.Length; i++)
        {
            GameObject _go = new GameObject("Sound" + i + " " + sounds[i].name);
            _go.transform.SetParent(this.transform);
            sounds[i].setSource(_go.AddComponent<AudioSource>());
        }
    }

    public void playSound(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].play();
                
                return;
            }
        }
        // the sound has not been found
        Debug.LogWarning("SoundManager: sound of name *" + _name + "* not found");
    }

    public void playSound(string _name, float volume)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].play();
                sounds[i].setVolume(volume);
                return;
            }
        }
        // the sound has not been found
        Debug.LogWarning("SoundManager: sound of name *" + _name + "* not found");
    }

    public void playSpatialSound(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].SetSpatialBlend(1f);
                sounds[i].play();
                return;
            }
        }
        // the sound has not been found
        Debug.LogWarning("SoundManager: sound of name *" + _name + "* not found");
    }

    public void StopSound(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Stop();
                return;
            }
        }
        // the sound has not been found
        Debug.LogWarning("SoundManager: sound of name *" + _name + "* not found");
    }

    public bool IsPlaying(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                return sounds[i].IsPlaying();
            }
        }
        // the sound has not been found
        Debug.LogWarning("SoundManager: sound of name *" + _name + "* not found");
        return false;
    }

    public void ChangeVolume(string _name, float change)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].ChangeVolume(change);
                return;
            }
        }
        // the sound has not been found
        Debug.LogWarning("SoundManager: sound of name *" + _name + "* not found");
    }

    public bool HasVolume(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                return sounds[i].HasVolume();
            }
        }
        // the sound has not been found
        Debug.LogWarning("SoundManager: sound of name *" + _name + "* not found");
        return false;
    }
}
