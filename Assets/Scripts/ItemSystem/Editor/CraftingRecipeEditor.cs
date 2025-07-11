using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Cosmobot.ItemSystem.Editor
{
    public class CraftingRecipeEditor : EditorWindow
    {
        private const string PrefsKeyFilePath = "cosmos_crafting_recipe_editor_file_path";

        private const float ListWidthPercent = 0.3f;
        private CraftingRecipeSerializationGroup currentGroup;

        [CanBeNull]
        private string filePath  ;

        private Vector2 groupListScrollPosition = Vector2.zero;
        private Vector2 grouprecipesAvailableListScrollPosition = Vector2.zero;
        private Vector2 grouprecipesInListScrollPosition = Vector2.zero;

        private bool groupSelected  ;
        private Vector2 recipeInputScrollPosition = Vector2.zero;

        private Vector2 recipeListScrollPosition = Vector2.zero;
        private Vector2 recipeOutputScrollPosition = Vector2.zero;

        private bool recipeselected  ;
        private int selectedRecipeIndex = -1;

        private RecipeEditorTab selectedTab = RecipeEditorTab.recipes;

        private CraftingRecipeSerializationObject serializationObject  ;
        private float DetailWidth => position.width * (1 - ListWidthPercent);

        // do not use when recipeselected is false
        private CraftingRecipe CurrentRecipe
        {
            get => serializationObject.recipes[selectedRecipeIndex];
            set => serializationObject.recipes[selectedRecipeIndex] = value;
        }

        public void OnGUI()
        {
            OnTitleBarGUI();
            OnEditorGUI();
        }


        [MenuItem("Cosmo/Crafting Recipe Editor")]
        private static void ShowWindow()
        {
            var window = GetWindow<CraftingRecipeEditor>("Crafting Recipe Editor");
            window.Show();
        }

        private void OnTitleBarGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Crafting Recipe Editor", EditorStyles.largeLabel);

            if (GUILayout.Button(SelectFileButtonText(), EditorStyles.toolbarButton))
            {
                Open();
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                Save();
            }

            GUILayout.EndHorizontal();
        }


        private void OnEditorGUI()
        {
            if (filePath is null)
            {
                GUILayout.Label("No file selected");
                return;
            }

            selectedTab = (RecipeEditorTab)GUILayout.SelectionGrid(
                (int)selectedTab, new[] { "recipes", "Groups" }, 2, EditorStyles.toolbarButton);

            GUILayout.Space(10);
            switch (selectedTab)
            {
                case RecipeEditorTab.recipes: OnRecipeGUI(); break;
                case RecipeEditorTab.groups: OnGroupsGUI(); break;
                default:
                    Debug.Log("Unknown tab selected. Defaulting to recipes.");
                    selectedTab = RecipeEditorTab.recipes;
                    OnRecipeGUI();
                    break;
            }
        }

        #region GuiHelpers

        // ========================================
        // GUI Helpers
        // ========================================

        private void TwoColumnGUI(float split, float availableWidth, Action onLeftColumnGUI, Action onRightColumnGUI)
        {
            float listRectWidth = availableWidth * split;
            float detailRectWidth = availableWidth - listRectWidth - 1;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(listRectWidth));
            onLeftColumnGUI();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(1);

            EditorGUILayout.BeginVertical(GUILayout.Width(detailRectWidth));
            onRightColumnGUI();
            EditorGUILayout.EndVertical();
            Rect lastRect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(lastRect.x - 2, lastRect.y, 2, lastRect.height), Color.black);

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region RecipeGUI

        // ========================================
        // Recipe GUI
        // ========================================

        private void OnRecipeGUI()
        {
            TwoColumnGUI(ListWidthPercent, position.width, OnRecipeListGUI, OnRecipeDetailGUI);
        }

        private void OnRecipeListGUI()
        {
            GUILayout.Label("recipes", EditorStyles.boldLabel);

            recipeListScrollPosition = GUILayout.BeginScrollView(recipeListScrollPosition);
            for (int i = 0; i < serializationObject.recipes.Count; i++)
            {
                CraftingRecipe recipe = serializationObject.recipes[i];
                string buttonName = $"{recipe.name} ({recipe.id})";
                // todo: rewrite this to use a button style
                if (i == selectedRecipeIndex) buttonName = $"> {buttonName} <";
                if (GUILayout.Button(buttonName, EditorStyles.toolbarButton))
                {
                    selectedRecipeIndex = i;
                    recipeselected = true;
                }
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add recipe"))
            {
                AddNewRecipe();
            }
        }

        private void OnRecipeDetailGUI()
        {
            GUILayout.Label("Recipe Details", EditorStyles.boldLabel);
            if (!recipeselected)
            {
                EditorGUILayout.LabelField("Select a recipe to edit");
                return;
            }

            CraftingRecipe currentRecipe = CurrentRecipe;
            currentRecipe.id = EditorGUILayout.TextField("ID", currentRecipe.id);
            currentRecipe.name = EditorGUILayout.TextField("Name", currentRecipe.name);
            currentRecipe.energyCost = EditorGUILayout.IntField("Energy Cost", currentRecipe.energyCost);

            TwoColumnGUI(0.5f, DetailWidth,
                () => OnRecipeDetailItemListGUI(currentRecipe.ingredients, "Ingredients",
                    ref recipeInputScrollPosition),
                () => OnRecipeDetailItemListGUI(currentRecipe.result, "Result", ref recipeOutputScrollPosition)
            );

            CurrentRecipe = currentRecipe;
        }

        private void OnRecipeDetailItemListGUI(List<string> list, string label, ref Vector2 scrollPosition)
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            float lineHeight = EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = EditorGUILayout.TextField(list[i]);
                if (GUILayout.Button("-", GUILayout.Width(lineHeight), GUILayout.Height(lineHeight)))
                {
                    list.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Add " + label))
            {
                list.Add("");
            }
        }

        #endregion

        #region GroupsGUI

        // ========================================
        // Groups GUI
        // ========================================

        private void OnGroupsGUI()
        {
            TwoColumnGUI(0.25f, position.width, OnGroupsListGUI, OnGroupsDetailGUI);
        }

        private void OnGroupsListGUI()
        {
            GUILayout.Label("Groups", EditorStyles.boldLabel);

            groupListScrollPosition = GUILayout.BeginScrollView(groupListScrollPosition);
            foreach (CraftingRecipeSerializationGroup group in serializationObject.groups)
            {
                string buttonName = $"{group.name} ({group.id})";
                // todo: rewrite this to use a button style
                if (currentGroup?.id == group.id) buttonName = $"> {buttonName} <";
                if (GUILayout.Button(buttonName, EditorStyles.toolbarButton))
                {
                    currentGroup = group;
                    groupSelected = true;
                }
            }

            if (GUILayout.Button("Add Group"))
            {
                AddNewGroup();
            }

            GUILayout.EndScrollView();
        }

        private void OnGroupsDetailGUI()
        {
            GUILayout.Label("Group Details", EditorStyles.boldLabel);
            if (!groupSelected)
            {
                EditorGUILayout.LabelField("Select a group to edit");
                return;
            }

            currentGroup.id = EditorGUILayout.TextField("ID", currentGroup.id);
            currentGroup.name = EditorGUILayout.TextField("Name", currentGroup.name);

            OnGroupsDetailrecipeselectorGUI();
        }

        private void OnGroupsDetailrecipeselectorGUI()
        {
            TwoColumnGUI(0.5f, DetailWidth,
                OnGroupsDetailrecipeselectorInListGUI,
                OnGroupsDetailrecipeselectorAvailableListGUI);
        }

        private void OnGroupsDetailrecipeselectorInListGUI()
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;

            GUILayout.Label("recipes in group", EditorStyles.boldLabel);
            grouprecipesInListScrollPosition = EditorGUILayout.BeginScrollView(grouprecipesInListScrollPosition);
            for (int i = 0; i < currentGroup.recipes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(currentGroup.recipes[i]);
                if (GUILayout.Button("-", GUILayout.Width(lineHeight), GUILayout.Height(lineHeight)))
                {
                    currentGroup.recipes.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnGroupsDetailrecipeselectorAvailableListGUI()
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;

            GUILayout.Label("Available recipes", EditorStyles.boldLabel);
            grouprecipesAvailableListScrollPosition =
                EditorGUILayout.BeginScrollView(grouprecipesAvailableListScrollPosition);
            IEnumerable<string> availablerecipes =
                serializationObject.recipes
                    .Select(r => r.id)
                    .Except(currentGroup.recipes);

            foreach (string recipe in availablerecipes)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(lineHeight), GUILayout.Height(lineHeight)))
                {
                    currentGroup.recipes.Add(recipe);
                }

                EditorGUILayout.LabelField(recipe);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region AssetManagment

        // ========================================
        // Asset Management
        // ========================================

        private void Open()
        {
            filePath = EditorPrefs.GetString(PrefsKeyFilePath);
            filePath = EditorUtility.SaveFilePanel(
                "Choose Crafting Recipe File", filePath ?? "", "recipes.json", "json");

            if (string.IsNullOrEmpty(filePath)) return;
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "{}", Encoding.UTF8);
            }

            serializationObject = CraftingRecipeSerializer.Deserialize(filePath);

            // save filePath in editor config
            EditorPrefs.SetString(PrefsKeyFilePath, filePath);
        }

        private void Save()
        {
            if (serializationObject is null || filePath is null)
            {
                Debug.LogError("Cannot save null serialization object");
                return;
            }

            CraftingRecipeSerializer.Serialize(filePath, serializationObject);
        }

        private string SelectFileButtonText()
        {
            return filePath is null ? "Select file" : Path.GetFileName(filePath);
        }

        private void AddNewRecipe()
        {
            CraftingRecipe currentRecipe = new();
            currentRecipe.ingredients = new List<string>();
            currentRecipe.result = new List<string>();

            serializationObject.recipes.Add(currentRecipe);
            selectedRecipeIndex = serializationObject.recipes.Count - 1;
            recipeselected = true;
        }

        private void AddNewGroup()
        {
            CraftingRecipeSerializationGroup group = new();
            group.recipes = new List<string>();
            serializationObject.groups.Add(group);
            currentGroup = group;
            groupSelected = true;
        }

        #endregion
    }

    internal enum RecipeEditorTab
    {
        recipes,
        groups
    }
}
