﻿//======================================================================
//        Copyright (C) 2015-2020 Winddy He. All rights reserved
//        Email: hgplan@126.com
//======================================================================
using System;
using System.Collections.Generic;
using UnityEngine;
using WindHotfix.Game;
using Framework;

namespace Game.Knight
{
    public class SystemMove : TGameSystem<ComponentMove, ComponentTransform, ComponentCollider>
    {
        protected override void OnUpdate(ComponentMove rCompMove, ComponentTransform rCompTrans, ComponentCollider rCompColl)
        {
            rCompMove.MoveSpeed.Normalize();

            // 地面的碰撞监测
            this.CheckGroundStatus(rCompMove, rCompTrans);
            // 计算旋转
            this.ApplyExtraRatation(rCompMove, rCompTrans);

            // 速度在平面上的投影
            rCompMove.MoveSpeed = Vector3.ProjectOnPlane(rCompMove.MoveSpeed, rCompMove.GroundNormal);

            rCompMove.TurnAmount = Mathf.Atan2(rCompMove.MoveSpeed.x, rCompMove.MoveSpeed.z);
            rCompMove.ForwardAmount = rCompMove.MoveSpeed.z;

            // 和墙面的碰撞检测
            this.MoveRayForword(rCompMove, rCompTrans, rCompColl);
            
            // 设置位置
            rCompTrans.Position += rCompMove.MoveSpeed * 0.075f * rCompMove.SpeedRate;
            rCompTrans.Position = new Vector3(rCompTrans.Position.x, rCompMove.GroundedY, rCompTrans.Position.z);
        }

        private void CheckGroundStatus(ComponentMove rCompMove, ComponentTransform rCompTrans)
        {
            RaycastHit rHitInfo;
            Vector3 rPos = rCompTrans.Position + rCompTrans.Up * 1.0f;
            if (Physics.Raycast(rPos, Vector3.down, out rHitInfo, rCompMove.GroundCheckDistance))
            {
                rCompMove.GroundNormal = rHitInfo.normal;
                rCompMove.GroundedY = rHitInfo.point.y - 0.1f;
            }
            else
            {
                rCompMove.GroundNormal = Vector3.up;
                rCompMove.GroundedY = 0;
            }
        }

        private void ApplyExtraRatation(ComponentMove rCompMove, ComponentTransform rCompTrans)
        {
            float fTurnSpeed = Mathf.Lerp(rCompMove.StationaryTurnSpeed, rCompMove.MovingTurnSpeed, rCompMove.ForwardAmount);
            rCompTrans.Forward = Vector3.RotateTowards(rCompTrans.Forward, rCompMove.MoveSpeed, fTurnSpeed, 0);
        }

        private void MoveRayForword(ComponentMove rCompMove, ComponentTransform rCompTrans, ComponentCollider rCompColl)
        {
            RaycastHit rHitInfo;
            Vector3 rActorPos = rCompTrans.Position + Vector3.up * 0.2f;
            float rActorRadius = rCompColl.Radius + 0.3f;

            if (Physics.Raycast(rActorPos, rCompTrans.Forward, out rHitInfo, rActorRadius, 1 << LayerMask.NameToLayer("Wall")))
            {
                rCompMove.MoveSpeed = Vector3.ProjectOnPlane(rCompMove.MoveSpeed, rHitInfo.normal);
                int k = Vector3.Dot(rCompTrans.Forward, rCompMove.MoveSpeed) >= 0 ? 1 : -1;
                rCompMove.MoveSpeed = rCompMove.MoveSpeed * k;
            }
        }
    }

    public class SystemAnimatorMove : TGameSystem<ComponentMove, ComponentAnimator>
    {
        protected override void OnUpdate(ComponentMove rCompMove, ComponentAnimator rCompAnim)
        {
            rCompAnim.IsMove = !rCompMove.MoveSpeed.Equals(Vector3.zero);
            rCompMove.SpeedRate = rCompAnim.IsRun ? 2.0f : 1.0f;
        }
    }

    public class SystemAnimatorInput : TGameSystem<ComponentAnimator, ComponentInput>
    {
        protected override void OnUpdate(ComponentAnimator rCompAnim, ComponentInput rCompInput)
        {
            rCompAnim.IsRun = rCompInput.IsRunInput;
        }
    }

    public class SystemInputMove : TGameSystem<ComponentMove, ComponentInput>
    {
        protected override void OnUpdate(ComponentMove rCompMove, ComponentInput rCompInput)
        {
            // 取到摄像机的方向
            var rCameraEntity = Utilities.GetEntityCamera();
            var rCameraTrans = rCameraEntity.CompCamera.Camera.transform;
            var rCamForward = Vector3.Scale(rCameraTrans.forward, rCompInput.TempForword);

            rCompInput.HorizontalInput = InputManager.Instance.Horizontal;
            rCompInput.VerticalInput = InputManager.Instance.Vertical;
            rCompInput.IsRunInput = InputManager.Instance.IsKey(InputKey.Run);
            
            rCompMove.MoveSpeed = rCompInput.VerticalInput * rCamForward + rCompInput.HorizontalInput * rCameraTrans.right;
        }
    }
}
