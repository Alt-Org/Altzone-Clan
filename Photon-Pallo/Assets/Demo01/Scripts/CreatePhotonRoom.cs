using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Demo01.Scripts
{
    public class CreatePhotonRoom : MonoBehaviourPunCallbacks
    {
        public TMP_Text _photonStatus;

        private string _curStatus;
        private string _prevStatus;
        
        private IEnumerator Start()
        {
            for (; enabled;)
            {
                if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
                {
                    PhotonNetwork.NickName = "player";
                    PhotonNetwork.GameVersion = string.Empty;
                    Debug.Log($"-> {PhotonNetwork.NetworkClientState} -> PhotonNetwork.ConnectUsingSettings");
                    if (PhotonNetwork.ConnectUsingSettings())
                    {
                        PhotonNetwork.GameVersion = Application.version;
                    }
                }
                else if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                {
                    Debug.Log($"-> {PhotonNetwork.NetworkClientState} -> PhotonNetwork.JoinLobby");
                    PhotonNetwork.JoinLobby();
                    yield break;
                }
                yield return null;
            }            
        }
        
        private void Update()
        {
            if (PhotonNetwork.InRoom)
            {
                _curStatus = $"=  {PhotonNetwork.GameVersion} {PhotonNetwork.NetworkClientState} p {PhotonNetwork.CurrentRoom.Players.Count}";
            }
            else if (PhotonNetwork.InLobby)
            {
                _curStatus = $"+  {PhotonNetwork.GameVersion} {PhotonNetwork.NetworkClientState} p {PhotonNetwork.CountOfPlayers}";
            }
            else
            {
                _curStatus = $"?  {PhotonNetwork.GameVersion} {PhotonNetwork.NetworkClientState} wait";
            }
            if (_prevStatus != _curStatus)
            {
                _prevStatus = _curStatus;
                _photonStatus.text = _curStatus;
                Debug.Log(_curStatus);
            }
        }
        public override void OnJoinedLobby()
        {
            Debug.Log($"-> {PhotonNetwork.NetworkClientState} -> PhotonNetwork.JoinRandomRoom");
            if (!PhotonNetwork.JoinRandomOrCreateRoom())
            {
                _curStatus = $"{PhotonNetwork.NetworkClientState} JoinRandomOrCreateRoom failed";
                _photonStatus.text = _curStatus;
                Debug.LogError(_curStatus);
                enabled = false;
            }
        }

        public override void OnCreatedRoom()
        {
            Debug.Log($"!  {PhotonNetwork.NetworkClientState} OnCreatedRoom");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"!  {PhotonNetwork.NetworkClientState} OnJoinedRoom");
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            _curStatus = $"{PhotonNetwork.NetworkClientState} Error {returnCode} {message}";
            _photonStatus.text = _curStatus;
            Debug.LogError(_curStatus);
            enabled = false;
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            _curStatus = $"{PhotonNetwork.NetworkClientState} Error {returnCode} {message}";
            _photonStatus.text = _curStatus;
            Debug.LogError(_curStatus);
            enabled = false;
        }
    }
}
