// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.HybridGraph
{
    public class HybridGraph : IDisposable
    {
        private INode  _currentNode;
        private bool   _disposed;
        private INode? _nextNode;

        private HybridGraph(IContainer container)
        {
            _currentNode = container.RootNode;
            _currentNode.OnEnterInternal();

            if (_nextNode is not null)
            {
                Update();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HybridGraph));
            }

            _disposed = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="blackboard"></param>
        /// <typeparam name="T"></typeparam>
        public static HybridGraph Run<T>(object blackboard)
            where T : class, IContainer, new()
        {
            // Initialize container (StateMachine or BehaviourTree)
            var container = new T();
            container.Initialize(blackboard, null);

            // Run container
            return new HybridGraph(container);
        }

        /// <summary>
        /// </summary>
        public void Update()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HybridGraph));
            }

            if (_nextNode is null)
            {
                // 現在のStateのUpdate関数を呼ぶ
                _currentNode.OnStayInternal();
            }

            // 次の遷移先が代入されていたら, ステートを切り替える
            while (_nextNode is not null)
            {
                // 以前のステートを終了する
                _currentNode.OnExitInternal(_nextNode);

                // ステートの切り替え処理
                _currentNode = _nextNode;
                _nextNode = null;

                // 次のステートを開始する
                _currentNode.OnEnterInternal();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public bool SendTrigger(int trigger)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HybridGraph));
            }

            if (_currentNode.TryGetNextNode(trigger, out var nextNode))
            {
                _nextNode = nextNode;
                return true;
            }

            return false;
        }
    }
}