﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2016/11/11

using DG.DeExtensions;
using DG.Tweening;
using UnityEngine;

namespace EA4S
{
    public class TutorialUIMark : TutorialUIProp
    {
        #region Unity

        protected override void Awake()
        {
            base.Awake();

            ShowTween.Kill();
            Img.SetAlpha(0);
            ShowTween = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(Img.DOFade(1, 0.2f))
                .Join(this.transform.DOPunchScale(Vector3.one * 0.4f * TutorialUI.GetCameraBasedScaleMultiplier(this.transform.position), 0.4f, 10, 0))
                .Join(this.transform.DOPunchRotation(new Vector3(0, 0, 30), 0.4f))
                .Append(Img.transform.DOLocalRotate(new Vector3(0, 0, -10), 0.7f).SetEase(Ease.Linear))
                .Insert(0.8f, this.transform.DOScale(0.0001f, 0.3f).SetEase(Ease.InBack))
                .OnComplete(() => {
                    this.gameObject.SetActive(false);
                    this.transform.parent = DefParent;
                });
        }

        #endregion

        #region Public Methods

        internal void SetSize(TutorialUI.MarkSize _size)
        {
            switch (_size) {
            case TutorialUI.MarkSize.Normal:
                Img.transform.localScale = Vector3.one;
                break;
            case TutorialUI.MarkSize.Big:
                Img.transform.localScale = Vector3.one * 1.5f;
                break;
            case TutorialUI.MarkSize.Huge:
                Img.transform.localScale = Vector3.one * 2;
                break;
            }
        }

        #endregion
    }
}