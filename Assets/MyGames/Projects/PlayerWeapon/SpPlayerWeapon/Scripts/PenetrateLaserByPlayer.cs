using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Trigger;
using WeaponUtility;

namespace SpPlayerWeapon
{
    public class PenetrateLaserByPlayer : SpPlayerWeapon
    {
        LaserUtility _laserUtility;
        SpWeaponType _type = SpWeaponType.LASER;

        public override SpWeaponType Type => _type;

        void Awake()
        {
            _laserUtility = GetComponent<LaserUtility>();
        }

        public override void Use()
        {
            _laserUtility.Use(_playerTransform);
        }
    }
}
