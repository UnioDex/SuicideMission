﻿using System.Collections;
using SuicideMission.Behavior;
using SuicideMission.Enums;
using SuicideMission.Interface;
using UnityEngine;

namespace SuicideMission.Objects
{
    public class Player : Spaceship
    {
        [Header("Player Specific Specs")]
        [SerializeField] private float firingDelay = 0.1f;
        [SerializeField] private float padding = 0.75f;

        [SerializeField] private float weaponHeatFactor = 20;
        [SerializeField] private float weaponHeatWaitingTime = 25f;
        [SerializeField] private float weaponHeatReduceFactor = 6;
        [SerializeField] private GameObject heatIndicator;

        private float shootingSpeedBoost = 1f;
        private float shootingSpeedBoostRemainingTime = 0f;

        private float laserSizeBoost = 1f;
        private float laserSizeBoostRemainingTime = 0f;

        private float trippleLaserBoostRemainingTime = 0f;

        private Coroutine firingCoroutine;

        private float weaponHeat;

        // Movement Boundaries
        private float xMin;
        private float xMax;
        private float yMin;
        private float yMax;

        protected override void Start()
        {
            base.Start();
            SetupMoveBoundaries();
        }

        protected override void Update()
        {
            Move();
            Fire();
            UpdateBoostTimers();

            heatIndicator.transform.localScale = new Vector3(1, Mathf.Clamp(weaponHeat / weaponHeatFactor, 0, 1), 1);
        }

        private void UpdateBoostTimers()
        {
            if (shootingSpeedBoostRemainingTime > 0) shootingSpeedBoostRemainingTime -= Time.deltaTime;

            if (laserSizeBoostRemainingTime > 0) laserSizeBoostRemainingTime -= Time.deltaTime;

            if (shootingSpeedBoostRemainingTime <= 0)
            {
                weaponHeatFactor /= shootingSpeedBoost;
                weaponHeatWaitingTime /= shootingSpeedBoost;
                shootingSpeedBoostRemainingTime = 0;
                shootingSpeedBoost = 1f;
            }

            if (laserSizeBoostRemainingTime <= 1)
            {
                laserSizeBoostRemainingTime = 0;
                laserSizeBoost = 1f;
            }

            if (trippleLaserBoostRemainingTime > 0)
                trippleLaserBoostRemainingTime -= Time.deltaTime;
            else
                trippleLaserBoostRemainingTime = 0;
        }

        private void Move()
        {
            var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
            var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;
            var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
            var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);
            transform.position = new Vector2(newXPos, newYPos);
        }

        protected override void Fire()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                firingCoroutine = StartCoroutine(FireContinuously());
                return;
            }

            if (Input.GetButtonUp("Fire1"))
            {
                StopCoroutine(firingCoroutine);
                return;
            }

            if (weaponHeat >= 0) weaponHeat -= Time.deltaTime * weaponHeatReduceFactor;
        }

        private IEnumerator FireContinuously()
        {
            while (true)
            {
                Shoot(Direction.Up);
                yield return new WaitForSeconds(firingDelay / shootingSpeedBoost);
            }
        }

        protected override void Shoot(Direction direction)
        {
            if (weaponHeat >= weaponHeatFactor) return;

            var laserBeanCount = 1;
            if (trippleLaserBoostRemainingTime > 0) laserBeanCount = 3;

            for (var i = 0; i < laserBeanCount; i++)
            {
                var laserBean = Instantiate(laser, transform.position, laser.transform.rotation);
                laserBean.transform.localScale *= laserSizeBoost;

                if (laserBeanCount == 3)
                    switch (i)
                    {
                        case 0:
                        {
                            var pos = laserBean.transform.position;
                            pos.x -= 0.3f;
                            pos.y -= 0.2f;
                            laserBean.transform.position = pos;
                            break;
                        }
                        case 2:
                        {
                            var pos = laserBean.transform.position;
                            pos.x += 0.3f;
                            pos.y -= 0.2f;
                            laserBean.transform.position = pos;
                            break;
                        }
                    }

                laserBean.GetComponent<DamageDealer>().SetDamage(projectileDamage);
                laserBean.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed * (int) direction);
                Destroy(laserBean, destroyBulletAfterSeconds);
            }

            HeatWeapon();
            AudioSource.PlayClipAtPoint(shootSound, cameraPosition, shootSoundVolume);
        }

        private void HeatWeapon()
        {
            weaponHeat++;
            if (weaponHeat >= weaponHeatFactor) weaponHeat = weaponHeatWaitingTime;
        }

        private void SetupMoveBoundaries()
        {
            var gameCamera = Camera.main;

            xMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + padding;
            xMax = gameCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - padding;

            yMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + padding;
            yMax = gameCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - padding;
        }

        protected override void Death()
        {
            base.Death();
            FindObjectOfType<Level>().LoadGameOver();
        }

        public void GiveSpeedBoost(float boost, float duration)
        {
            if (shootingSpeedBoostRemainingTime <= 0 || shootingSpeedBoost != boost)
            {
                weaponHeatFactor *= boost;
                weaponHeatWaitingTime *= boost;

                shootingSpeedBoost = boost;
                shootingSpeedBoostRemainingTime = duration;
            }
            else
            {
                shootingSpeedBoostRemainingTime += duration;
            }
        }

        public void GiveLaserSizeBoost(float boost, float duration)
        {
            if (laserSizeBoostRemainingTime <= 0 || laserSizeBoost != boost)
            {
                laserSizeBoost = boost;
                laserSizeBoostRemainingTime = duration;
            }
            else
            {
                laserSizeBoostRemainingTime += duration;
            }
        }

        public void GiveTrippleLaserBoost(float duration)
        {
            trippleLaserBoostRemainingTime += duration;
        }
        
        public float GetFiringDelay()
        {
            return firingDelay;
        }

        public float SetFiringDelay(float firingDelay)
        {
            return this.firingDelay = firingDelay;
        }
    }
}