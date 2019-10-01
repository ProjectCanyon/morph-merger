/* 
MIT License

Copyright (c) 2019 ProjectCanyon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. 
*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace MorphMerger
{
    public class MorphMerger : MVRScript
    {
        private Atom _person;

        private UIDynamicButton _refreshButton;
        private UIDynamicButton _selectAllButton;
        private UIDynamicButton _selectNoneButton;
        private UIDynamicButton _mergeButton;

        private List<SelectableMorph> _mainBank;
        private List<SelectableMorph> _genitalBank;

        private DAZCharacterSelector _characterSelector;

        private UIDynamicTextField _titleTextField;
        private UIDynamicTextField _infoTextField;
        private UIDynamicTextField _resultTextField;

        private InputField _morphNameInputField;
        private InputField _groupNameInputField;
        private InputField _regionNameInputField;

        public override void Init()
        {
            try
            {
                pluginLabelJSON.val = "MorphMerger v1.0.5 (by ProjectCanyon)";

                if (containingAtom.type != "Person")
                {
                    SuperController.LogError($"This plugin is for use with 'Person' atom only, not '{containingAtom.type}'");
                    return;
                }

                _person = containingAtom;

                _characterSelector = _person.GetComponentInChildren<DAZCharacterSelector>();
                if (_characterSelector == null)
                    throw new InvalidOperationException("Missing DAZCharacterSelector");

                _titleTextField = CreateTextField(new JSONStorableString("info", "Morph Merger"));
                _titleTextField.height = 12;
                _titleTextField.UItext.fontSize = 72;
                _titleTextField.UItext.alignment = TextAnchor.LowerCenter;
                _titleTextField.UItext.fontStyle = FontStyle.Bold;

                var infoBuilder = new StringBuilder();

                infoBuilder.AppendLine();
                infoBuilder.AppendLine("Select morphs on the right to include them into the merged morph.");
                infoBuilder.AppendLine();
                infoBuilder.AppendLine("Morph colors are greener the more impact they are having.");
                infoBuilder.AppendLine();
                infoBuilder.AppendLine("Disable auto behaviours to prevent them showing.");
                infoBuilder.AppendLine();
                infoBuilder.AppendLine("Morph will not show up until hard reset via main menu, or VAM restart.");

                _infoTextField = CreateTextField(new JSONStorableString("info", infoBuilder.ToString()));
                _infoTextField.height = 450;
                _infoTextField.UItext.fontSize = 32;

                _mainBank = new List<SelectableMorph>();
                _genitalBank = new List<SelectableMorph>();

                _refreshButton = CreateButton("Refresh", rightSide: true);
                _refreshButton.buttonColor = Color.yellow;
                if (_refreshButton != null)
                    _refreshButton.button.onClick.AddListener(Refresh);

                _selectAllButton = CreateButton("Select All", rightSide: true);
                if (_selectAllButton != null)
                    _selectAllButton.button.onClick.AddListener(() =>
                    {
                        foreach (SelectableMorph selectableMorph in _mainBank.Concat(_genitalBank))
                            selectableMorph.Storable.val = selectableMorph.Selected = true;
                    });

                _selectNoneButton = CreateButton("Select None", rightSide: true);
                if (_selectNoneButton != null)
                    _selectNoneButton.button.onClick.AddListener(() =>
                    {
                        foreach (SelectableMorph selectableMorph in _mainBank.Concat(_genitalBank))
                            selectableMorph.Storable.val = selectableMorph.Selected = false;
                    });

                var morphNameLabel = CreateTextField(new JSONStorableString("morphName", "Morph Name"));
                morphNameLabel.UItext.fontSize = 36;
                morphNameLabel.backgroundColor = Color.clear;

                SetLayoutHeight(morphNameLabel, 48);

                var morphNameTextField = CreateTextField(new JSONStorableString(name, "My Morph"));
                _morphNameInputField = morphNameTextField.gameObject.AddComponent<InputField>();
                _morphNameInputField.textComponent = morphNameTextField.UItext;
                _morphNameInputField.text = "My Morph";
                morphNameTextField.height = 18;
                morphNameTextField.backgroundColor = Color.white;
                _morphNameInputField.textComponent.fontSize = 36;

                SetLayoutHeight(morphNameTextField, 48);

                var groupNameLabel = CreateTextField(new JSONStorableString("groupName", "Group"));
                groupNameLabel.height = 12;
                groupNameLabel.UItext.fontSize = 36;
                groupNameLabel.backgroundColor = Color.clear;

                SetLayoutHeight(groupNameLabel, 48);

                var groupNameTextField = CreateTextField(new JSONStorableString(name, "_MorphMerger"));
                _groupNameInputField = groupNameTextField.gameObject.AddComponent<InputField>();
                _groupNameInputField.textComponent = groupNameTextField.UItext;
                _groupNameInputField.text = "_MorphMerger";
                groupNameTextField.height = 18;
                groupNameTextField.backgroundColor = Color.white;
                _groupNameInputField.textComponent.fontSize = 36;

                SetLayoutHeight(_groupNameInputField, 48);

                var regionNameLabel = CreateTextField(new JSONStorableString("regionName", "Region (Category)"));
                regionNameLabel.height = 12;
                regionNameLabel.UItext.fontSize = 36;
                regionNameLabel.backgroundColor = Color.clear;

                SetLayoutHeight(regionNameLabel, 48);

                var regionNameTextField = CreateTextField(new JSONStorableString(name, "_MorphMerger"));
                _regionNameInputField = regionNameTextField.gameObject.AddComponent<InputField>();
                _regionNameInputField.textComponent = regionNameTextField.UItext;
                _regionNameInputField.text = "_MorphMerger";
                regionNameTextField.height = 18;
                regionNameTextField.backgroundColor = Color.white;
                _regionNameInputField.textComponent.fontSize = 36;

                SetLayoutHeight(regionNameTextField, 48);

                _mergeButton = CreateButton("Merge");
                _mergeButton.buttonColor = Color.red;
                _mergeButton.height = 150;
                _mergeButton.buttonText.fontStyle = FontStyle.Bold;
                _mergeButton.buttonText.fontSize = 42;
                if (_mergeButton != null)
                    _mergeButton.button.onClick.AddListener(Merge);

                _resultTextField = CreateTextField(new JSONStorableString("result", string.Empty));
                _resultTextField.UItext.fontSize = 24;

                Refresh();
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void SetLayoutHeight(Component component, float height)
        {
            var layoutElement = component.GetComponent<LayoutElement>();
            layoutElement.minHeight = 0f;
            layoutElement.preferredHeight = height;
        }

        private UIDynamic _genitalSpacer;

        private void Refresh()
        {
            foreach (SelectableMorph selectableMorph in _mainBank)
                RemoveToggle(selectableMorph.Storable);

            _mainBank.Clear();

            foreach (SelectableMorph selectableMorph in _genitalBank)
                RemoveToggle(selectableMorph.Storable);

            _genitalBank.Clear();

            ScanBank(_characterSelector.morphBank1, _mainBank);

            if (_genitalSpacer != null)
                RemoveSpacer(_genitalSpacer);
            _genitalSpacer = CreateSpacer(rightSide: true);

            ScanBank(_characterSelector.morphBank2, _genitalBank);
            ScanBank(_characterSelector.morphBank3, _genitalBank);
        }

        private void ScanBank(DAZMorphBank bank, List<SelectableMorph> selectableList)
        {
            if (bank == null)
                return;

            foreach (DAZMorph morph in bank.morphs.OrderByDescending(NormalisedMagnitude))
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (morph.appliedValue != morph.jsonFloat.defaultVal && morph.visible)
                {
                    var jsonStorable = new JSONStorableBool($"{morph.morphName}Bool", false);

                    var morphToggle = CreateToggle(jsonStorable, true);
                    morphToggle.label = morph.displayName;

                    var selectable = new SelectableMorph
                    {
                        Morph = morph,
                        Storable = jsonStorable,
                    };

                    morphToggle.backgroundColor = Color.Lerp(Color.white, Color.green, NormalisedMagnitude(morph));
                    morphToggle.toggle.onValueChanged.AddListener(value =>
                    {
                        selectable.Selected = value;
                    });

                    selectableList.Add(selectable);
                }
            }
        }
        
        private void Merge()
        {
            try
            {
                var rndId = Guid.NewGuid().ToString("N").Substring(0, 8);
                
                ClearResultField();

                var characterSelector = _person.GetComponentInChildren<DAZCharacterSelector>();
                if (characterSelector == null)
                    throw new InvalidOperationException("Missing DAZCharacterSelector");

                string morphName = _morphNameInputField.text;
                if (string.IsNullOrEmpty(morphName))
                    morphName = _person.name;

                // Main
                DAZMorph mainMorph = ProcessMorphBank(_mainBank, morphName, morphName);
                if (mainMorph != null)
                {
                    string mainMetaPath = characterSelector.morphBank1.autoImportFolder + $"/{morphName}-{rndId}.vmi";
                    string mainDeltasPath = characterSelector.morphBank1.autoImportFolder + $"/{morphName}-{rndId}.vmb";

                    var mainMeta = mainMorph.GetMetaJSON();
                    if (mainMeta == null)
                        throw new InvalidOperationException("Failed to generate meta data");

                    SaveJSON(mainMeta, mainMetaPath);
                    mainMorph.SaveDeltasToBinaryFile(mainDeltasPath);

                    WriteToResultField($"Saved VMI: \"{mainMetaPath}\"");
                    WriteToResultField($"Saved VMB \"{mainDeltasPath}\"");
                }

                // Genital
                DAZMorph genMorph = ProcessMorphBank(_genitalBank, $"{morphName}-genital", $"{morphName} Genital");
                if (genMorph != null)
                {
                    string genMetaPath = characterSelector.morphBank2.autoImportFolder + $"/{morphName}-{rndId}-Genital.vmi";
                    string genDeltasPath = characterSelector.morphBank2.autoImportFolder + $"/{morphName}-{rndId}-Genital.vmb";

                    var genMeta = genMorph.GetMetaJSON();
                    if (genMeta == null)
                        throw new InvalidOperationException("Failed to generate gen meta data");

                    SaveJSON(genMeta, genMetaPath);
                    genMorph.SaveDeltasToBinaryFile(genDeltasPath);

                    WriteToResultField($"Saved Genital VMI: \"{genMetaPath}\"");
                    WriteToResultField($"Saved Genital VMB: \"{genDeltasPath}\"");
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void ClearResultField()
        {
            _resultTextField.text = string.Empty;
        }

        private void WriteToResultField(string message)
        {
            _resultTextField.text += Environment.NewLine + message;
        }

        private DAZMorph ProcessMorphBank(IEnumerable<SelectableMorph> bank, string name, string displayName)
        {
            try
            {
                var vertexDeltas = new Dictionary<int, Vector3>();
                var formulas = new Dictionary<string, Dictionary<DAZMorphFormulaTargetType, DAZMorphFormula>>();

                foreach (SelectableMorph selectableMorph in bank)
                {
                    if (!selectableMorph.Selected)
                        continue;

                    if (selectableMorph.Morph.deltas != null)
                    {
                        foreach (DAZMorphVertex delta in selectableMorph.Morph.deltas)
                        {
                            if (!vertexDeltas.ContainsKey(delta.vertex))
                                vertexDeltas.Add(delta.vertex, delta.delta * selectableMorph.Morph.morphValue);
                            else
                                vertexDeltas[delta.vertex] += delta.delta * selectableMorph.Morph.morphValue;
                        }
                    }

                    foreach (DAZMorphFormula formula in selectableMorph.Morph.formulas)
                    {
                        if (formula.targetType == DAZMorphFormulaTargetType.MCM || formula.targetType == DAZMorphFormulaTargetType.MCMMult)
                            continue;

                        var combinedFormula = new DAZMorphFormula
                        {
                            targetType = formula.targetType,
                            target = formula.target,
                            multiplier = formula.multiplier * selectableMorph.Morph.morphValue
                        };

                        if (!formulas.ContainsKey(combinedFormula.target))
                            formulas.Add(combinedFormula.target, new Dictionary<DAZMorphFormulaTargetType, DAZMorphFormula> { { combinedFormula.targetType, combinedFormula } });
                        else if (!formulas[combinedFormula.target].ContainsKey(combinedFormula.targetType))
                            formulas[combinedFormula.target].Add(combinedFormula.targetType, combinedFormula);
                        else
                            formulas[combinedFormula.target][combinedFormula.targetType].multiplier += combinedFormula.multiplier;
                    }
                }

                if (vertexDeltas.Count == 0 && formulas.Count == 0)
                    return null;

                var groupName = _groupNameInputField.text;
                if (string.IsNullOrEmpty(groupName))
                    groupName = "_MorphMerger";

                var regionName = _regionNameInputField.text;
                if (string.IsNullOrEmpty(regionName))
                    regionName = "_MorphMerger";

                var combinedMorph = new DAZMorph
                {
                    group = groupName,
                    region = regionName,
                    morphName = name,
                    displayName = displayName,
                    min = 0,
                    max = 1,
                    visible = true,
                    disable = false,
                    isPoseControl = false,
                    formulas = formulas
                        .SelectMany(x => x.Value.Values)
                        .ToArray(),
                    numDeltas = vertexDeltas.Count,
                    deltas = vertexDeltas
                        .Select(x => new DAZMorphVertex { vertex = x.Key, delta = x.Value })
                        .ToArray(),
                };

                return combinedMorph;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }

            return null;
        }

        public static float NormalisedMagnitude(DAZMorph morph)
        {
            return morph.morphValue > morph.jsonFloat.defaultVal ?
                Math.Abs(morph.jsonFloat.defaultVal - morph.morphValue) / Math.Abs(morph.jsonFloat.defaultVal - morph.max) :
                Math.Abs(morph.jsonFloat.defaultVal - morph.morphValue) / Math.Abs(morph.jsonFloat.defaultVal - morph.min);
        }

        public class SelectableMorph
        {
            public bool Selected { get; set; }
            public DAZMorph Morph { get; set; }
            public JSONStorableBool Storable { get; set; }
        }

        public class SelectableMorphComparer : IComparer<SelectableMorph>
        {
            public int Compare(SelectableMorph x, SelectableMorph y)
            {
                if (x == null || y == null) return 0;
                return NormalisedMagnitude(x.Morph).CompareTo(MorphMerger.NormalisedMagnitude(y.Morph));
            }
        }
    }
}