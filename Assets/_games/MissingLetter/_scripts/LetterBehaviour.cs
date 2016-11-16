﻿using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;
using TMPro;
using System;
using System.Collections.Generic;
using ArabicSupport;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Plugins.Core.PathCore;

namespace EA4S.MissingLetter
{

    public class LetterBehaviour : MonoBehaviour
    {

        void Start()
        {
            //Assert.IsNotNull<LetterObjectView>(mLetter, "LetterView Not Set in " + name);
            mCollider = gameObject.GetComponent<Collider>();
            Assert.IsNotNull<Collider>(mCollider, "Collider Not Set in " + name);
            mCollider.enabled = false;
            mbIsSpeaking = false;
            mLetter.SetWalkingSpeed(1);
        }

        public void PlayAnimation(LLAnimationStates animation)
        {
            mLetter.SetState(animation);
        }

        void MoveTo(Vector3 position, float duration, bool IdleAtEnd = true)
        {
            PlayAnimation(LLAnimationStates.LL_walking);
            mLetter.SetWalkingSpeed(1);

            if (moveTweener != null)
            {
                moveTweener.Kill();
            }

            moveTweener = transform.DOLocalMove(position, duration).OnComplete(
                delegate () {
                    if (IdleAtEnd)
                        PlayAnimation(m_oDefaultIdleAnimation);
                    if (endTransformToCallback != null)
                        endTransformToCallback();
                });
        }

        void RotateTo(Vector3 rotation, float duration)
        {
            if (rotationTweener != null)
            {
                rotationTweener.Kill();
            }
            rotationTweener = transform.DORotate(rotation, duration);
        }

        void OnEndLifeCycle()
        {
            Reset();

            if (onLetterBecameInvisible != null)
            {
                onLetterBecameInvisible(gameObject);
            }
        }

        void OnMouseDown()
        {
            Debug.Log("Answer Clicked: " + mLetterData.Key);

            Speak();

            if (onLetterClick != null)
            {
                StartCoroutine(Utils.LaunchDelay(0.2f, onLetterClick, mLetterData.Key));
                mCollider.enabled = false;
            }
        }

        private Vector3 CalculatePos(int _idxPos, int maxItemsInScreen)
        {
            float _zeroPosX = mv3CenterPosition.x + 1.0f; // +1.0 beacuse we have some UI Widget at the left

            if (maxItemsInScreen % 2 == 0)
            {
                _zeroPosX += mfDistanceBetweenLetters / 2;
            }

            Vector3 _GoalPos;

            if (_idxPos == 0)
            {
                _GoalPos = new Vector3(_zeroPosX, mv3CenterPosition.y, mv3CenterPosition.z);
            }
            else
            {
                int _n = ((_idxPos + 1) / 2);
                if (_idxPos % 2 != 0)
                {
                    _GoalPos = new Vector3(_zeroPosX - (_n * mfDistanceBetweenLetters), mv3CenterPosition.y, mv3CenterPosition.z);
                }
                else
                {
                    _GoalPos = new Vector3(_zeroPosX + (_n * mfDistanceBetweenLetters), mv3CenterPosition.y, mv3CenterPosition.z);
                }
            }
            return _GoalPos;
        }


        #region INTERFACE

        public void Reset() {
            gameObject.transform.position = mv3StartPosition;
            gameObject.transform.rotation = Quaternion.identity;
            endTransformToCallback = null;
            onLetterClick = null;
            LightOff();
        }

        public void SetPositions(Vector3 start, Vector3 center, Vector3 end)
        {
            mv3StartPosition = start;
            mv3CenterPosition = center;
            mv3EndPosition = end;
        }

        public ILivingLetterData LetterData
        {
            get
            {
                return mLetterData;
            }
            set
            {
                mLetterData = value;
                mLetter.Init(value);
            }
        }

        public void Refresh()
        {
            mLetter.Init(mLetterData);
        }

        public void EnterScene(int _idxPos = 0, int maxItemsInScreen = 1)
        {
            Vector3 dir = (mv3CenterPosition - mv3StartPosition).normalized;
            Vector3 _GoalPos = CalculatePos(_idxPos, maxItemsInScreen);
            endTransformToCallback += delegate { mCollider.enabled = true; };

            //move and rotate letter
            gameObject.transform.forward = dir;
            endTransformToCallback += delegate { RotateTo(Vector3.up * 180, 0.5f); };

            MoveTo(_GoalPos, 1);
        }

        public void ExitScene()
        {

            LightOff();

            onLetterClick = null;
            endTransformToCallback = null;
            endTransformToCallback += OnEndLifeCycle;
            mCollider.enabled = false;

            Vector3 dir = (mv3EndPosition - mv3CenterPosition).normalized;

            Vector3 rot = new Vector3(0, Vector3.Angle(Vector3.forward, dir), 0);
            rot = (Vector3.Cross(Vector3.forward, dir).y < 0) ? -rot : rot;
            RotateTo(rot, 1f);

            MoveTo(mv3EndPosition, 1);
        }

        public void ChangePos(int _idxPos, int maxItemsInScreen, float duration)
        {

            LightOff();

            mCollider.enabled = false;
            Vector3 newPos = CalculatePos(_idxPos, maxItemsInScreen);

            Vector3 dist = (gameObject.transform.position - newPos) / 2;

            Vector3 pivot = gameObject.transform.position - dist;

            //dist is only on x
            float radius = dist.x + 0.1f;

            float accuracy = 4f;
            for (int i = 1; i <= accuracy; ++i)
            {
                Vector3 p = Vector3.zero;
                p += pivot;
                p.x += Mathf.Cos(3.14f * (i / accuracy)) * radius;
                p.z += Mathf.Sin(3.14f * (i / accuracy)) * radius;

                positions.Add(p);
            }


            PlayAnimation(LLAnimationStates.LL_walking);
            mLetter.SetWalkingSpeed(1);
            mLetter.HasFear = true;

            transform.DOLookAt(positions[0], 1f);

            TweenerCore<Vector3, Path, PathOptions> value = transform.DOPath(positions.ToArray(), duration, PathType.CatmullRom);
            value.OnWaypointChange(delegate (int wayPoint) {
                if (wayPoint < positions.Count)
                    transform.DOLookAt(positions[wayPoint], 1f);
            });
            value.OnComplete(delegate {
                transform.DOLookAt(transform.position + Vector3.back, 1f);
                positions.Clear();
                PlayAnimation(m_oDefaultIdleAnimation);
                mCollider.enabled = true;
            });
        }


        public void Speak()
        {
            Debug.Log("Speaking the word: " + mLetterData.Key);
            if (mLetterData != null && !mbIsSpeaking)
            {
                mbIsSpeaking = true;
                if (mLetterData.DataType == LivingLetterDataType.Letter)
                {
                    AudioManager.I.PlayLetter(mLetterData.Key);
                }
                else
                {
                    AudioManager.I.PlayWord(mLetterData.Key);
                }
                StartCoroutine(Utils.LaunchDelay(0.8f, SetIsSpeaking, false));
            }
        }

        private void SetIsSpeaking(bool _isSpeaking)
        {
            mbIsSpeaking = _isSpeaking;
        }

        public void SetEnableCollider(bool _enabled) {
            mCollider.enabled = _enabled;
        }

        public void SuggestLetter()
        {
            PlayAnimation(LLAnimationStates.LL_dancing);
            //mLetter.DoHighFive();
            LightOn();
        }

        
        public void LightOn() {
            if (spotLight == null) {
                spotLight = new GameObject("SpotLight");

                spotLight.transform.position = gameObject.transform.position + Vector3.up * 8 + Vector3.back * 6;
                spotLight.transform.Rotate(Vector3.right, 50);
                spotLight.transform.parent = gameObject.transform;
                Light cSpotLight = spotLight.AddComponent<Light>();
                cSpotLight.type = LightType.Spot;
                cSpotLight.range = 40;
                cSpotLight.intensity = 8;
                cSpotLight.spotAngle = 50;
            }
        }
        
        public void LightOff() {
            if (spotLight != null) {
                Destroy(spotLight);
                spotLight = null;
            }
                
        }

        public void StopSuggest()
        {
            LightOff();
        }

        #endregion

        #region VARS

        private List<Vector3> positions = new List<Vector3>();

        private Tweener moveTweener;
        private Tweener rotationTweener;
        private Collider mCollider;

        private GameObject spotLight;
        private bool mbIsSpeaking;

        public LetterObjectView mLetter;

        [HideInInspector]
        public ILivingLetterData mLetterData;
        [HideInInspector]
        public Action endTransformToCallback;
        [HideInInspector]
        public Action<string> onLetterClick;
        [HideInInspector]
        public event Action<GameObject> onLetterBecameInvisible;

        //public for pool
        [HideInInspector]
        public float mfDistanceBetweenLetters;
        [HideInInspector]
        public Vector3 mv3StartPosition;
        [HideInInspector]
        public Vector3 mv3CenterPosition;
        [HideInInspector]
        public Vector3 mv3EndPosition;


        public LLAnimationStates m_oDefaultIdleAnimation { get; set; }
        #endregion



    }
}
