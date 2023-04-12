using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";


    [SerializeField]private AudioClipRefsSO audioClipRefsSO;

    private float volume;

    private void Awake() {
        Instance = this;

        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME , 1f);
    }

    private void Start() {
        DeliveryManager.Instance.OnDeliverySuccess += DeliveryManager_OnDeliverySuccess;
        DeliveryManager.Instance.OnDeliveryFail += DeliveryManager_OnDeliveryFail;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        Player.OnAnyPickedupObject += Player_OnAnyPickedupObject;
        BaseCounter.OnAnyObjectDroped += BaseCounter_OnAnyObjectDroped;
        TrashCounter.OnAnyTrashed += TrashCounter_OnAnyTrashed;
    }

    private void TrashCounter_OnAnyTrashed(object sender, System.EventArgs e){
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(audioClipRefsSO.trash , trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectDroped(object sender, System.EventArgs e){
        BaseCounter baseCounter = sender as BaseCounter;
        PlaySound(audioClipRefsSO.objectDrop , baseCounter.transform.position);
    }

    private void Player_OnAnyPickedupObject(object sender, System.EventArgs e){
        Player player = sender as Player;
        PlaySound(audioClipRefsSO.objectPickup , player.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e){
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(audioClipRefsSO.chop , cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnDeliveryFail(object sender, System.EventArgs e){
        PlaySound(audioClipRefsSO.deliveryFail , DeliveryManager.Instance.transform.position);
    }

    private void DeliveryManager_OnDeliverySuccess(object sender, System.EventArgs e){
        PlaySound(audioClipRefsSO.deliverySuccess , DeliveryManager.Instance.transform.position);
    }

    private void PlaySound(AudioClip[] audioClipArray , Vector3 position , float volume = 1f){
        PlaySound(audioClipArray[Random.Range(0,audioClipArray.Length)] , position , volume);
    }

    private void PlaySound(AudioClip audioClip,Vector3 position , float volumeMultiplier = 1f){
        AudioSource.PlayClipAtPoint(audioClip , position , volumeMultiplier * volume);
    }

    public void PlayFootStepSound(Vector3 position , float volume = 1f){
        PlaySound(audioClipRefsSO.footStep , position , volume);
    }

    public void PlayCountDownSound(){
        PlaySound(audioClipRefsSO.warning , Vector3.zero);
    }

    public void PlayWarningSound(Vector3 position){
        PlaySound(audioClipRefsSO.warning , position);
    }

    public void ChangeVolume(){
        volume += .1f;
        if(volume > 1){
            volume = 0f;
        }
        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME , volume);
        PlayerPrefs.Save();
    }

    public float GetVolume(){
        return volume;
    }
}
