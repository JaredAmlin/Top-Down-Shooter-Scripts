using CCS.Health.HealthBar;
using CCS.Pickup.Armor;
using CCS.Player;
using CCS.Types;
using System;
using UnityEngine;

public class ArmorPickup : MonoBehaviour
{
    #region Variables

    [Header("Scriptable Object")]
    [SerializeField] private PU_ArmorData _armorData;

    private int _previousID;
    private int _currentID;

    [Header("Audio")]
    [SerializeField] private AudioClip _pUSoundEffect;

    [Header("Particles")]
    [SerializeField] private ParticleSystem _armorParticles;

    //cached string names
    private const string _playerTag = "Player";
    private const string _mechTag = "Mech";

    private Player _player;
    private Mech _mech;

    #endregion

    #region Collision

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            UpdateShieldColor();

            _player = PlayerManager.Instance.GetPlayer();
            _player.SetFireState();

            Instantiate(_armorParticles, other.transform.position, Quaternion.identity);
            AudioManager.Instance.PlaySound(_pUSoundEffect, AudioManager.Instance.SFX);

            if (_player._playerID is 0 or 1)
            {

                PlayerManager.Instance.SetPlayerState(_armorData._playerID);
                _player.UpdateIcon(_player.GetWeaponsLevel());

                Destroy(this.gameObject);
                return;
            }
            else if (_player._playerID is 2 or 3)
            {
                if (_armorData._playerID != 4)
                    _player.RestoreHealth(_player._maxHealth);

                PlayerManager.Instance.SetPreviousID(_armorData._playerID);

                if (_armorData._playerID != _player._playerID)
                {
                    PlayerManager.Instance.SetPlayerState(_armorData._playerID);
                }


                Destroy(this.gameObject);
                return;
            }
        }
        else if (other.CompareTag(_mechTag))
        {
            UpdateShieldColor();

            _mech = PlayerManager.Instance.GetMechState();


            _mech.UpdateIcon();
            UIManager.Instance.ArmorFill.fillAmount = 1;
            UIManager.Instance.AnimateHealthSystem(UIManager.Instance.ArmorHealthSystem);

            //if pickup is armor type
            //assign previous ID to armor type
            if (_armorData.playerState is PlayerState.RedArmor or PlayerState.BlueArmor)
            {
                PlayerManager.Instance.ArmorHealed();

                //store previous armor ID
                PlayerManager.Instance.SetPreviousID(_armorData._playerID);

                AudioManager.Instance.PlaySound(_pUSoundEffect, AudioManager.Instance.SFX);
                Instantiate(_armorParticles, other.transform.position, Quaternion.identity);

                Destroy(this.gameObject);
                return;
            }
            else
            {
                PlayerManager.Instance.ArmorHealed();
                UIManager.Instance.AnimateHealthSystem(UIManager.Instance.ArmorHealthSystem);
                Destroy(this.gameObject);
                return;
            }
        }
    }

    #endregion

    public void UpdateShieldColor()
    {
        if (_armorData.playerState == PlayerState.RedArmor)
        {
            UIManager.Instance.ActivateShieldSystemAndChangeSpriteColor(Color.red, true);
        }
        if (_armorData.playerState == PlayerState.BlueArmor)
        {
            UIManager.Instance.ActivateShieldSystemAndChangeSpriteColor(Color.blue, true);
        }
    }
}
