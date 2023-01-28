// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using NUnit.Framework;

namespace Edanoue.StateMachine.Tests
{
    using VM = VendingMachine.VendingMachine;

    /// <summary>
    /// StateMachine テスト用の自動販売機クラスに対するテストクラス
    /// 自動販売機クラスそのものというよりかは
    /// StateMachine の挙動に重きをおいてテストを書いています
    /// </summary>
    public class VendingMachineTest
    {
        [Test]
        public void 自動販売機生成のテスト()
        {
            var vm = new VM();

            // 生成されている
            Assert.NotNull(vm);

            // 最初のState は Lockedになっている
            Assert.That(vm.CurrentState, Is.EqualTo("StateLocked"));
        }

        [Test]
        public void 自動販売機に1枚お金を入れるとお金足りない状態()
        {
            var vm = new VM();

            // お金を 一枚 入れると StateNotEnoughMoney 状態
            vm.EnterCoin(1);
            Assert.That(vm.CurrentState, Is.EqualTo("StateNotEnoughMoney"));
        }

        [Test]
        public void 自動販売機に3枚お金を入れるとお金足りてる状態()
        {
            var vm = new VM();

            // お金を 一枚 入れると StateNotEnoughMoney 状態
            vm.EnterCoin(1);
            Assert.That(vm.CurrentState, Is.EqualTo("StateNotEnoughMoney"));

            // 3枚を超えると StateEnoughMoney 状態
            vm.EnterCoin(2);
            Assert.That(vm.CurrentState, Is.EqualTo("StateEnoughMoney"));

            // いきなり3枚を超過しても StateEnoughMoney 状態
            vm = new VM();
            vm.EnterCoin(5);
            Assert.That(vm.CurrentState, Is.EqualTo("StateEnoughMoney"));
        }

        [Test]
        public void お金を入れた状態でお釣り返却をする()
        {
            var vm = new VM();

            // 最初に押しても別にお釣り出てこない
            vm.PushButtonChange();
            Assert.That(vm.ProvidedCoinCount, Is.EqualTo(0));

            // お金2枚いれる
            vm.EnterCoin(2);

            // お金足りてない状態からお釣りの返却が可能
            vm.PushButtonChange();
            Assert.That(vm.TotalCoinCount, Is.EqualTo(0));
            Assert.That(vm.ProvidedCoinCount, Is.EqualTo(2));

            // お金3枚いれる
            vm.EnterCoin(3);
            Assert.That(vm.TotalCoinCount, Is.EqualTo(3));

            // お金足りてる状態からお釣りの返却が可能
            vm.PushButtonChange();
            Assert.That(vm.TotalCoinCount, Is.EqualTo(0));
            Assert.That(vm.ProvidedCoinCount, Is.EqualTo(5));

            // いつ押しても特に問題ない
            vm.PushButtonChange();
            Assert.That(vm.ProvidedCoinCount, Is.EqualTo(5));
        }

        [Test]
        public void ジュース購入ボタンを押す()
        {
            var vm = new VM();

            // 最初に押してもジュース出てこない
            vm.PushButtonJuice();
            Assert.That(vm.ProvidedJuiceCount, Is.EqualTo(0));

            // お金を2枚入れる
            vm.EnterCoin(2);
            // お金が足りないのでジュース出てこない
            vm.PushButtonJuice();
            Assert.That(vm.ProvidedJuiceCount, Is.EqualTo(0));

            // お金を2枚入れる
            vm.EnterCoin(2);
            // お金が足りるのでジュース出てくる
            vm.PushButtonJuice();
            Assert.That(vm.ProvidedJuiceCount, Is.EqualTo(1));
            // ついでにもうかえないのでお釣りも出てきている
            Assert.That(vm.ProvidedCoinCount, Is.EqualTo(1));
            // ロック状態に戻っている
            Assert.That(vm.CurrentState, Is.EqualTo("StateLocked"));

            // おかねを 6 枚入れる
            vm.EnterCoin(6);
            // お金が足りるのでジュース出てくる
            vm.PushButtonJuice();
            Assert.That(vm.ProvidedJuiceCount, Is.EqualTo(2));
            // まだおかねが残っているのでおつりは出てこない
            Assert.That(vm.ProvidedCoinCount, Is.EqualTo(1));
            // ジュース買う と出てくる
            vm.PushButtonJuice();
            Assert.That(vm.ProvidedJuiceCount, Is.EqualTo(3));
        }
    }
}