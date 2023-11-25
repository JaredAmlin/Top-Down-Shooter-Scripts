using CCS.Pickup.Weapons.Specialized;
using CCS.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPickUp : MonoBehaviour
{
    [SerializeField] private PU_Speed _speedData;
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

            _player.ActivateSpeedMultiplier(_speedData.speedMultiplier, _speedData.speedDurationValue);
            Destroy(this.gameObject);
        }

        if (other.CompareTag(_mechTag))
        {
            _mech = PlayerManager.Instance.GetMechState();

            _mech.ActivateSpeedMultiplier(_speedData.speedMultiplier, _speedData.speedDurationValue);

            Destroy(this.gameObject);
        }

    }
}

