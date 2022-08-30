// ----------------------------------------------------------------------------
// <copyright file="PhotonRigidbodyView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize rigidbodies via PUN.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using System;

namespace Photon.Pun
{
    using UnityEngine;


    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Photon Networking/Photon Rigidbody View")]
    public class PhotonRigidbodyView : MonoBehaviourPun, IPunObservable
    {
        private float m_Distance;
        private float m_Angle;

        [NonSerialized] public Rigidbody m_Body;

        private Vector3 m_NetworkPosition;

        private Quaternion m_NetworkRotation;
        
        private bool m_NetworkKinematic;
        private bool originKinematic;
        private bool pause;

        [HideInInspector]
        public bool m_SynchronizeVelocity = true;
        [HideInInspector]
        public bool m_SynchronizeAngularVelocity = false;

        [HideInInspector]
        public bool m_TeleportEnabled = false;
        [HideInInspector]
        public float m_TeleportIfDistanceGreaterThan = 3.0f;

        public void Awake()
        {
            this.m_Body = GetComponent<Rigidbody>();

            this.m_NetworkPosition = new Vector3();
            this.m_NetworkRotation = new Quaternion();
        }

        private void OnEnable()
        {
        
        }

        private void OnDisable()
        {
          
        }
        
      

        public void FixedUpdate()
        {
            if (PhotonNetwork.InRoom == false || PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            {
                return;
            }
            if (!this.photonView.IsMine)
            {
                if (m_Body.isKinematic)
                {
                    this.m_Body.position = this.m_NetworkPosition;
                    this.m_Body.rotation = this.m_NetworkRotation;
                }
                else
                {
                    this.m_Body.position = Vector3.MoveTowards(this.m_Body.position, this.m_NetworkPosition,
                        this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                    this.m_Body.rotation = Quaternion.RotateTowards(this.m_Body.rotation, this.m_NetworkRotation,
                        this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
                }
            }
            else if (!pause && m_Body.isKinematic != m_NetworkKinematic)
            {
                PhotonNetwork.RemoveBufferedRPCs(this.photonView.ViewID, "EnableRigid");
                photonView.RPC("EnableRigid",RpcTarget.OthersBuffered,m_Body.isKinematic);
                m_NetworkKinematic = m_Body.isKinematic;
            }
        }

        [PunRPC]
        public void EnableRigid(bool b)
        {
            m_NetworkKinematic = b;
            m_Body.isKinematic = b;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(this.m_Body.position);
                stream.SendNext(this.m_Body.rotation);

                if (this.m_SynchronizeVelocity)
                {
                    stream.SendNext(this.m_Body.velocity);
                }

                if (this.m_SynchronizeAngularVelocity)
                {
                    stream.SendNext(this.m_Body.angularVelocity);
                }
            }
            else
            {
                this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                if (this.m_TeleportEnabled)
                {
                    if (Vector3.Distance(this.m_Body.position, this.m_NetworkPosition) > this.m_TeleportIfDistanceGreaterThan)
                    {
                        this.m_Body.position = this.m_NetworkPosition;
                    }
                }
                
                if (this.m_SynchronizeVelocity || this.m_SynchronizeAngularVelocity)
                {
                    float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

                    if (this.m_SynchronizeVelocity)
                    {
                        this.m_Body.velocity = (Vector3)stream.ReceiveNext();

                        this.m_NetworkPosition += this.m_Body.velocity * lag;

                        this.m_Distance = Vector3.Distance(this.m_Body.position, this.m_NetworkPosition);
                    }

                    if (this.m_SynchronizeAngularVelocity)
                    {
                        this.m_Body.angularVelocity = (Vector3)stream.ReceiveNext();

                        this.m_NetworkRotation = Quaternion.Euler(this.m_Body.angularVelocity * lag) * this.m_NetworkRotation;

                        this.m_Angle = Quaternion.Angle(this.m_Body.rotation, this.m_NetworkRotation);
                    }
                }
            }
        }
    }
}