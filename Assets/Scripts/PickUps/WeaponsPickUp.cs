using CCS.Pickup.Weapons.Specialized;
using CCS.Player;
using System;
using UnityEngine;

public class WeaponsPickUp : MonoBehaviour
{
    [SerializeField] private PU_Specialized_Weapon _weaponsData;
    [SerializeField] private AudioClip _pUSoundEffect;
    [SerializeField] private GameObject _particleEffect;
    private const string _playerTag = "Player";
    private const string _mechTag = "Mech";
    private Player _player;
    private Mech _mech;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            _player = PlayerManager.Instance.GetPlayer();
            _player.ActivateSpecialWeapon(_weaponsData.weaponsID, _weaponsData.durationValue);
            UIManager.Instance.ActivateWeaponsIconSystem(_weaponsData.durationValue);
            
            Instantiate(_particleEffect, other.transform.position, Quaternion.identity, other.transform);
            Destroy(this.gameObject);
        }

        if (other.CompareTag(_mechTag))
        {
            _mech = PlayerManager.Instance.GetMechState();

            _mech.ActivateSpecialWeapon(_weaponsData.weaponsID, _weaponsData.durationValue);

            Instantiate(_particleEffect, other.transform.position, Quaternion.identity, other.transform);

            Destroy(this.gameObject);
        }
    }
}

