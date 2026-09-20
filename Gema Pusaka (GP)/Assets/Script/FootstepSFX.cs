using UnityEngine;

public class FootstepSFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;

 private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        bool isWalking = animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Walk");

       
        if (!isWalking && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    public void PlayFootstepSound()
    {
        if (footstepClips.Length == 0) return;
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip);
    }
}
