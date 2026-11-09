using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Tests
{
    public sealed class CheckoutSchedulingRuntimeTests
    {
        private const string User="33333333-3333-4333-8333-333333333333";
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        private GameObject root;private PaidBoxCheckoutService checkout;private bool hadPending;private string oldPending;
        private const string Pending="pockle.checkout.pending.v1."+User;
        private static FieldInfo F(Type type,string name)=>type.GetField(name,Private);
        private void Set(string name,object value)=>F(typeof(PaidBoxCheckoutService),name).SetValue(checkout,value);
        private object Get(string name)=>F(typeof(PaidBoxCheckoutService),name).GetValue(checkout);
        private void Call(string name,params object[] args)=>typeof(PaidBoxCheckoutService).GetMethod(name,Private).Invoke(checkout,args);
        [SetUp] public void SetUp()
        {
            hadPending=PlayerPrefs.HasKey(Pending);oldPending=PlayerPrefs.GetString(Pending);PlayerPrefs.DeleteKey(Pending);
            root=new GameObject("Checkout scheduling without network or store");checkout=root.AddComponent<PaidBoxCheckoutService>();
            var account=root.AddComponent<SupabaseAccountService>();var stored=F(typeof(SupabaseAccountService),"stored");
            stored.SetValue(account,JsonUtility.FromJson("{\"session\":{\"access_token\":\"fixture\",\"refresh_token\":\"fixture\",\"expires_at\":9999999999,\"user\":{\"id\":\""+User+"\"}}}",stored.FieldType));
            F(typeof(SupabaseAccountService),"ready").SetValue(account,true);
            Set("hud",root.AddComponent<PrototypeHud>());
            Set("account",account);Set("user",User);Set("working",true);
            var packages=(System.Collections.IDictionary)Get("packages");
            // Queueing must not dereference a store package or open native billing while another operation is active.
            packages.Add("pockle.mix.3.v1",null);packages.Add("pockle.mix.5.v1",null);
        }
        [TearDown] public void TearDown(){UnityEngine.Object.DestroyImmediate(root);if(hadPending)PlayerPrefs.SetString(Pending,oldPending);else PlayerPrefs.DeleteKey(Pending);PlayerPrefs.Save();}
        [Test] public void MixQuoteWaitsForBackgroundWorkAndKeepsOnlyTheNewestSelection()
        {
            Call("RequestQuote","pockle.mix.3.v1",null);Assert.That(Get("queuedQuote"),Is.EqualTo("pockle.mix.3.v1"));
            Call("RequestQuote","pockle.mix.5.v1",null);Assert.That(Get("queuedQuote"),Is.EqualTo("pockle.mix.5.v1"));
            Assert.That(Get("storeOpen"),Is.False);
        }
        [Test] public void ExistingUncertainPurchaseRequestsRecoveryInsteadOfAnotherPayment()
        {
            PlayerPrefs.SetString(Pending,"{\"state\":\"uncertain\",\"product\":\"pockle.mix.3.v1\"}");
            Call("RequestQuote","pockle.mix.5.v1",null);
            Assert.That(Get("recoverSoon"),Is.True);Assert.That(Get("queuedQuote"),Is.Null);Assert.That(Get("storeOpen"),Is.False);
            Assert.That(PlayerPrefs.GetString(Pending),Does.Contain("uncertain"));
        }
        [Test] public void ConfirmationAndDeliveryRecoveryAreNotLostDuringBackgroundWork()
        {
            Set("quoteId","quote-fixture");Set("quoteProduct","pockle.mix.3.v1");Call("Confirm","quote-fixture");
            Assert.That(Get("queuedConfirmation"),Is.EqualTo("quote-fixture"));Assert.That(Get("storeOpen"),Is.False);
            Call("RecoverRequested");Assert.That(Get("recoverSoon"),Is.True);Assert.That(Get("recoveryAt"),Is.EqualTo(0f));
            Assert.That(PlayerPrefs.HasKey("pockle.checkout.pending.v1."+User),Is.False,"Queueing cannot claim a payment or spend anything.");
        }
    }
}
