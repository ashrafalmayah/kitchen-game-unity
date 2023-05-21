using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour,IKitchenObjectParent
{
    public static Player LocalInstance { get; private set; }
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickedupObject;
    public static event EventHandler OnAnyPickupOrDrop;
    public static void ResetStaticDate(){
        OnAnyPlayerSpawned = null;
        OnAnyPickedupObject = null;
        OnAnyPickupOrDrop = null;
    }


    public event EventHandler OnPickedupObject;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public BaseCounter selectedCounter;
    }
    [SerializeField]private float moveSpeed = 7f;
    [SerializeField]private GameInput gameInput;
    [SerializeField]private LayerMask CountersLayer;
    [SerializeField]private LayerMask CollisionsLayer;
    [SerializeField]private Transform playerObjectHoldPoint;
    [SerializeField]private List<Vector3> playerPositionList;

    private KitchenObject kitchenObject;

    private bool isWalking;
    private BaseCounter selectedCounter;
    private Vector3 lastInteractDirection;


    private void Start() {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAciton;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAciton;
    }


    public override void OnNetworkSpawn(){
        if(IsOwner){
            LocalInstance = this;
        }

        transform.position = playerPositionList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];


        OnAnyPlayerSpawned?.Invoke(this , EventArgs.Empty);

        if(IsServer){
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId){
        if(clientId == OwnerClientId && HasKitchenObject()){
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void GameInput_OnInteractAlternateAciton(object sender, EventArgs e){
        if(!GameManager.Instance.IsPlaying()) return;

        if(selectedCounter != null){
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAciton(object sender , System.EventArgs e){
        if(!GameManager.Instance.IsPlaying()) return;
        
        if(selectedCounter != null){
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if(!IsOwner){
            return;
        }
        
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking(){
        return isWalking;
    }

    private void HandleInteractions(){
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVector.x,0f,inputVector.y);

        if(moveDirection != Vector3.zero){
            lastInteractDirection = moveDirection;
        }

        float interactDistance = 2f;
        if(Physics.Raycast(transform.position , lastInteractDirection , out RaycastHit raycastHit , interactDistance , CountersLayer)){
            if(raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)){
                if(selectedCounter != baseCounter){
                    SetSelectedCounter(baseCounter);
                }
            }else{
                SetSelectedCounter(null);
            }
        }else{
            SetSelectedCounter(null);
        }
        
    }
    private void HandleMovement(){
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVector.x,0f,inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        bool canMove = !Physics.BoxCast(transform.position , Vector3.one *  playerRadius , moveDirection , Quaternion.identity , moveDistance , CollisionsLayer);
        Vector3 rotationDirection = moveDirection;
        
        if(!canMove){
            //can't move
            //Attempt moving in only X axis
            Vector3 moveDirectionX = new Vector3(moveDirection.x,0,0);
            canMove = (moveDirection.x > 0.125 || moveDirection.x < -0.125) && !Physics.BoxCast(transform.position , Vector3.one *  playerRadius , moveDirectionX , Quaternion.identity , moveDistance , CollisionsLayer);
            if(canMove){
                moveDirection = moveDirectionX;
            }
            else{
                //Can't move in the X axis
                //Attempt moving in the only Z axis
                Vector3 moveDirectionZ = new Vector3(0,0,moveDirection.z);
                canMove = (moveDirection.z > 0.125 || moveDirection.z < -0.125) && !Physics.BoxCast(transform.position , Vector3.one * playerRadius , moveDirectionZ , Quaternion.identity , moveDistance , CollisionsLayer);
                if(canMove){
                    moveDirection = moveDirectionZ;
                }else{
                    //Can't move in any Axis
                }

            }
        }
        if(canMove){
        transform.position += moveDirection * moveDistance;
        }

        isWalking = moveDirection != Vector3.zero;
        
        float rotateSpeed = 10f;
        if(isWalking)
        transform.forward = Vector3.Slerp(transform.forward,rotationDirection,Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter){
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
            selectedCounter = selectedCounter
        });
    }

    public BaseCounter GetSelectedCounter(){
        return selectedCounter;
    }

    public Transform GetKitchenObjectFollowTransform(){
        return playerObjectHoldPoint;
    }
    
    public KitchenObject GetKitchenObject(){
        return kitchenObject;
    }

    public void SetKitchenObject(KitchenObject kitchenObject){
        this.kitchenObject = kitchenObject;
        OnPickedupObject?.Invoke(this , EventArgs.Empty);
        OnAnyPickedupObject?.Invoke(this , EventArgs.Empty);
        OnAnyPickupOrDrop?.Invoke(this , EventArgs.Empty);
    }

    public void ClearKitchenObject(){
        this.kitchenObject = null;
        OnAnyPickupOrDrop?.Invoke(this , EventArgs.Empty);
    }

    public bool HasKitchenObject(){
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject(){
        return NetworkObject;
    }
}
