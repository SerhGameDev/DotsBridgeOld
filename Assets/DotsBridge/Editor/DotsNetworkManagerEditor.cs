//using UnityEditor;
//using UnityEngine;
//using DotsBridge.Modules.Network;

//namespace DotsBridge.Editor
//{
//    // Указываем, что этот скрипт меняет отрисовку DotsNetworkManager
//    [CustomEditor(typeof(DotsNetworkManager))]
//    public class DotsNetworkManagerEditor : UnityEditor.Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            // Отрисовываем стандартные поля (IP, Port, TickRate и т.д.)
//            DrawDefaultInspector();

//            var manager = (DotsNetworkManager)target;

//            EditorGUILayout.Space(10);
//            EditorGUILayout.LabelField("Управление сетью (Только в Play Mode)", EditorStyles.boldLabel);

//            // Кнопки должны быть активны только когда игра запущена
//            GUI.enabled = Application.isPlaying;

//            // Рисуем кнопки в один ряд (горизонтально)
//            GUILayout.BeginHorizontal();

//            // Зеленоватая кнопка для сервера
//            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
//            if (GUILayout.Button("Запустить Сервер", GUILayout.Height(35)))
//            {
//                manager.StartServer();
//            }

//            // Голубоватая кнопка для клиента
//            GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
//            if (GUILayout.Button("Подключить Клиента", GUILayout.Height(35)))
//            {
//                manager.ConnectToServer();
//            }

//            GUILayout.EndHorizontal();

//            // Возвращаем стандартные цвета и активность
//            GUI.backgroundColor = Color.white;
//            GUI.enabled = true;

//            if (!Application.isPlaying)
//            {
//                EditorGUILayout.HelpBox("Кнопки станут активны после нажатия кнопки Play в Unity.", MessageType.Info);
//            }
//        }
//    }
//}