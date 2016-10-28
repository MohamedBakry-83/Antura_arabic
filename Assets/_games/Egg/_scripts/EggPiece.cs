﻿using System;
using UnityEngine;

namespace EA4S.Egg
{
    public class EggPiece : MonoBehaviour
    {
        public Action onPoofEnd;

        public MeshRenderer pieceRenderer;

        public Rigidbody eggRigidbody;

        public GameObject poofPrefab;

        Vector3 poofRight = new Vector3(10f, 10f);
        Vector3 poofLeft = new Vector3(-10f, 10f);

        bool landed = true;
        const float landedTime = 0.3f;
        float landedTimer = 0f;

        bool poofed = false;

        bool poofDirRight = true;

        bool smoke = false;
        const float smokeTime = 2f;
        float smokeTimer = 0f;

        public void Reset()
        {
            gameObject.SetActive(true);

            eggRigidbody.useGravity = false;
            eggRigidbody.velocity = Vector3.zero;

            transform.localPosition = Vector3.zero;

            pieceRenderer.enabled = true;

            poofed = false;

            landed = true;
            landedTimer = 0f;

            smoke = false;
            smokeTimer = 0f;
        }

        public void Poof(bool poofDirRight)
        {
            if (!poofed)
            {
                poofed = true;

                this.poofDirRight = poofDirRight;

                MoveAndPoof();
            }
        }

        void MoveAndPoof()
        {
            eggRigidbody.useGravity = true;

            eggRigidbody.velocity = poofDirRight ? poofRight : poofLeft;

            landed = false;
            landedTimer = landedTime;
        }

        void Update()
        {
            if(!landed)
            {
                landedTimer -= Time.deltaTime;

                if(landedTimer <= 0f)
                {
                    landed = true;

                    eggRigidbody.useGravity = false;
                    eggRigidbody.velocity = Vector3.zero;

                    pieceRenderer.enabled = false;

                    StartSmoke();
                }
            }

            if(smoke)
            {
                smokeTimer -= Time.deltaTime;

                if(smokeTimer <= 0f)
                {
                    smoke = false;

                    OnSmokeEnd();
                }
            }
        }

        void StartSmoke()
        {
            smoke = true;

            smokeTimer = smokeTime;

            var poofGo = GameObject.Instantiate(poofPrefab);
            poofGo.AddComponent<AutoDestroy>().duration = smokeTime;
            poofGo.SetActive(true);

            poofGo.transform.SetParent(transform);
            poofGo.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        }

        void OnSmokeEnd()
        {
            if (onPoofEnd != null)
            {
                onPoofEnd();
            }
        }
    }
}
