using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Turrets
{
    public class Turret : MonoBehaviour
    {
        public GameObject beamPrefab;

        private GameObject head;
        private GameObject gun;
        private GameObject shootPoint;
        private ParticleSystem muzzleFlash;

        private Quaternion mainRotation;
        private Quaternion mainSmoothRotation;

        private Transform target;

        private float shootTimer;

        private void Start()
        {
            head = gameObject.FindChild("Base").FindChild("Pylon").FindChild("Head");
            gun = head.FindChild("Gun");
            shootPoint = gun.FindChild("ShootPoint");
            muzzleFlash = shootPoint.FindChild("MuzzleFlash").GetComponent<ParticleSystem>();

            StartCoroutine(SearchForTargets());
        }

        private void Update()
        {
            if (target == null)
            {
                // Does a simple spin-around animation, looks cool to see 
                // An indicator that the turret currently isn't doing anything.
                DoIdleAnimation();
            }
            else
            {
                // Rotate the head and the main gun of the turret to face the target
                RotateHead(target.position);

                // Not time to shoot!
                if (Time.time < shootTimer) return;

                // Get the angle between our current rotation
                float angle = Mathf.Abs(Quaternion.Angle(mainSmoothRotation, mainRotation));

                if (angle < 15)
                {
                    if (Physics.Raycast(head.transform.position, (target.position - head.transform.position), out RaycastHit hitInfo, 100, -2097153))
                    {
                        if (hitInfo.collider.gameObject == target.gameObject)
                        {
                            Shoot();
                            shootTimer = Time.time + 0.5f;
                        }
                    }
                }
            }
        }

        private void Shoot()
        {
            muzzleFlash.Play();

            GameObject beam = Instantiate(beamPrefab);
            LineRenderer lineRenderer = beam.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, shootPoint.transform.position);
            lineRenderer.SetPosition(1, target.transform.position);

            Destroy(beam, 0.3f);
        }

        private void DoIdleAnimation()
        {
            head.transform.rotation = Quaternion.RotateTowards(head.transform.rotation, Quaternion.Euler(0, (head.transform.rotation.eulerAngles.y + 10), 0), 100 * Time.deltaTime);
            gun.transform.localRotation = Quaternion.RotateTowards(gun.transform.localRotation, Quaternion.Euler(0, 0, 0), 100 * Time.deltaTime);
        }

        private void RotateHead(Vector3 position)
        {
            mainRotation = Quaternion.LookRotation(position - head.transform.position, Vector3.up);

            var headRotation = Quaternion.Euler(0, mainRotation.eulerAngles.y, 0);
            var gunRotation = Quaternion.Euler(mainRotation.eulerAngles.x, 0, 0);

            mainSmoothRotation = Quaternion.RotateTowards(Quaternion.Euler(gun.transform.localRotation.eulerAngles.x, head.transform.rotation.eulerAngles.y, 0), mainRotation, 200 * Time.deltaTime);

            head.transform.rotation = Quaternion.Euler(0, mainSmoothRotation.eulerAngles.y, 0);
            gun.transform.localRotation = Quaternion.Euler(mainSmoothRotation.eulerAngles.x, 0, 0);
        }

        IEnumerator SearchForTargets()
        {
            while(true)
            {
                int hits = UWE.Utils.OverlapSphereIntoSharedBuffer(transform.position, 15, -1);
                GameObject currentTarget = null;
                float currentDist = 0;

                for (int i = 0; i < hits; i++)
                {
                    if(UWE.Utils.sharedColliderBuffer[i].gameObject != gameObject)
                    {
                        Creature creature = Utils.FindAncestorWithComponent<Creature>(UWE.Utils.sharedColliderBuffer[i].gameObject);

                        if (creature == null) continue;
                        if (creature.GetComponent<AttackLastTarget>() == null) continue;

                        float dist = Vector3.Distance(transform.position, creature.transform.position);

                        if (currentTarget == null || dist < currentDist)
                        {
                            currentTarget = creature.gameObject;
                            currentDist = dist;
                        }
                    }
                }

                target = currentTarget?.transform ?? null;

                yield return new WaitForSeconds(2);
            }
        }
    }
}
