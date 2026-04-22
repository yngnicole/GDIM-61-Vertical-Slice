using System.Collections;
using UnityEngine;

public class AutoTester : MonoBehaviour
{
    bool _running = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !_running)
            StartCoroutine(RunAutoTest());
    }

    IEnumerator RunAutoTest()
    {
        _running = true;
        Debug.Log("=== AUTO TEST START ===");

        var mgr = OrderManager.Instance;
        if (mgr == null) { Fail("OrderManager.Instance is null"); yield break; }
        Debug.Log("[AutoTest] OrderManager found.");

        // Wait for an order bubble
        Debug.Log("[AutoTest] Waiting for NPC bubble...");
        OrderBubble bubble = null;
        float elapsed = 0f;
        while (bubble == null && elapsed < 10f)
        {
            bubble = Object.FindObjectOfType<OrderBubble>();
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (bubble == null) { Fail("No OrderBubble found after 10s"); yield break; }
        Debug.Log("[AutoTest] PASS: Order bubble appeared.");
        yield return new WaitForSeconds(0.5f);

        // Step 1: Take order
        Debug.Log("[AutoTest] Step 1: Taking order...");
        mgr.TakeOrder(bubble.Owner);
        bubble.OnOrderTaken();
        if (!mgr.HasTakenOrder(bubble.Owner)) { Fail("Order not registered"); yield break; }
        Debug.Log("[AutoTest] PASS: Order taken. Pending: " + mgr.PendingOrderCount);
        yield return new WaitForSeconds(0.5f);

        // Step 2: Start brewing
        Debug.Log("[AutoTest] Step 2: Starting brew...");
        CoffeeMachine machine = Object.FindObjectOfType<CoffeeMachine>();
        if (machine == null) { Fail("No CoffeeMachine in scene"); yield break; }
        mgr.StartBrewing(machine);
        if (!machine.IsBrewing) { Fail("Machine not brewing"); yield break; }
        Debug.Log("[AutoTest] PASS: Brewing started on " + machine.gameObject.name);

        // Step 3: Wait for brew
        Debug.Log("[AutoTest] Step 3: Waiting for brew to complete...");
        elapsed = 0f;
        while (!machine.IsDrinkReady && elapsed < 10f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!machine.IsDrinkReady) { Fail("Brew did not complete in time"); yield break; }
        Debug.Log("[AutoTest] PASS: Brew complete!");
        yield return new WaitForSeconds(0.5f);

        // Step 4: Pick up drink
        Debug.Log("[AutoTest] Step 4: Picking up drink...");
        mgr.PickUpDrink();
        machine.OnPickedUp();
        if (!mgr.HoldingDrink) { Fail("Not holding drink after pickup"); yield break; }
        Debug.Log("[AutoTest] PASS: Drink picked up.");
        yield return new WaitForSeconds(0.5f);

        // Step 5: Deliver
        Debug.Log("[AutoTest] Step 5: Delivering order...");
        NPC activeNPC = mgr.ActiveNPC;
        if (activeNPC == null) { Fail("ActiveNPC is null"); yield break; }
        int moneyBefore = mgr.Money;
        mgr.FulfillOrder(activeNPC);
        if (mgr.HoldingDrink) { Fail("Still holding drink after delivery"); yield break; }
        if (mgr.Money <= moneyBefore) { Fail("Money didn't increase"); yield break; }
        Debug.Log("[AutoTest] PASS: Order delivered! Money: $" + mgr.Money);

        Debug.Log("=== AUTO TEST COMPLETE: ALL PASSED ===");
        _running = false;
    }

    void Fail(string msg)
    {
        Debug.LogError("[AutoTest] FAIL: " + msg);
        _running = false;
    }
}
