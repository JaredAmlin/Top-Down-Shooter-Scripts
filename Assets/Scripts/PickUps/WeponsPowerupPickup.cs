using CCS.Pickup.Weapons.Specialized;
using CCS.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeponsPowerupPickup : MonoBehaviour
{
    [SerializeField] private PU_Weapons_Powerup _powerup;
    [SerializeField] private AudioClip _pUSoundEffect;
    private const string _playerTag = "Player";
    private const string _mechTag = "Mech";
    private Player _player;
    private Mech _mech;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            _player = PlayerManager.Instance.GetPlayer();
            _player.WeaponUpgrade();
          
            Destroy(this.gameObject);
        }

        if (other.CompareTag(_mechTag))
        {
            _mech = PlayerManager.Instance.GetMechState();

            Destroy(this.gameObject);
        }
    }
}
