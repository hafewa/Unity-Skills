---
name: unity-uitoolkit
description: "UI Toolkit (UITK) for Unity — create/edit USS stylesheets and UXML layouts, configure UIDocument in scenes. Use when users want to create UI with UI Toolkit, UXML, USS, UIDocument, PanelSettings, or modern Unity UI. Triggers: UI Toolkit, UXML, USS, UIDocument, PanelSettings, VisualElement, 界面工具包."
---

# Unity UI Toolkit Skills

Work with Unity's web-style UI system: **UXML** (≈HTML structure) + **USS** (≈CSS styling) + **UIDocument** (scene display).

> **Requires Unity 2021.3+**. This module is separate from `ui_*` skills (uGUI/Canvas). Use `uitk_*` for UI Toolkit only.

## Skills Overview

| Skill | Category | Description |
|-------|----------|-------------|
| `uitk_create_uss` | File | Create USS stylesheet |
| `uitk_create_uxml` | File | Create UXML layout |
| `uitk_read_file` | File | Read USS/UXML content |
| `uitk_write_file` | File | Write/overwrite USS/UXML |
| `uitk_delete_file` | File | Delete USS/UXML file |
| `uitk_find_files` | File | Search USS/UXML in project |
| `uitk_create_document` | Scene | Create UIDocument GameObject |
| `uitk_set_document` | Scene | Modify UIDocument properties |
| `uitk_create_panel_settings` | Scene | Create PanelSettings asset |
| `uitk_list_documents` | Scene | List scene UIDocuments |
| `uitk_inspect_uxml` | Inspect | Parse UXML element hierarchy |
| `uitk_create_from_template` | Template | Create UXML+USS from template |
| `uitk_create_batch` | Batch | Batch create USS/UXML files |

---

## File Operation Skills

### uitk_create_uss
Create a new USS stylesheet. If `content` is omitted, generates a default template with CSS variables.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `savePath` | string | Yes | — | Asset path, e.g. `Assets/UI/HUD.uss` |
| `content` | string | No | template | Full USS content to write |

**Returns**: `{success, path, lines}`

---

### uitk_create_uxml
Create a new UXML layout file.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `savePath` | string | Yes | — | Asset path, e.g. `Assets/UI/HUD.uxml` |
| `content` | string | No | template | Full UXML content to write |
| `ussPath` | string | No | null | USS path to embed as `<Style src="..."/>` in default template |

**Returns**: `{success, path, lines}`

---

### uitk_read_file
Read the source content of a USS or UXML file.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `filePath` | string | Yes | Asset path to USS or UXML file |

**Returns**: `{path, type, lines, content}`

---

### uitk_write_file
Overwrite a USS or UXML file with new content (creates file if it doesn't exist).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `filePath` | string | Yes | Asset path to write |
| `content` | string | Yes | New file content |

**Returns**: `{success, path, lines}`

---

### uitk_delete_file
Delete a USS or UXML file.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `filePath` | string | Yes | Asset path to delete |

**Returns**: `{success, deleted}`

---

### uitk_find_files
Search for USS and/or UXML files in the project.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `type` | string | No | `"all"` | `"uss"`, `"uxml"`, or `"all"` |
| `folder` | string | No | `"Assets"` | Search root folder |
| `filter` | string | No | null | Substring filter on path |
| `limit` | int | No | 200 | Max results |

**Returns**: `{count, files: [{path, type, name}]}`

---

## Scene Operation Skills

### uitk_create_document
Create a new GameObject with a `UIDocument` component attached.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No | `"UIDocument"` | GameObject name |
| `uxmlPath` | string | No | null | VisualTreeAsset (.uxml) path |
| `panelSettingsPath` | string | No | null | PanelSettings (.asset) path |
| `sortOrder` | int | No | 0 | Rendering sort order |
| `parentName` | string | No | null | Parent GameObject name |
| `parentInstanceId` | int | No | 0 | Parent by instance ID |
| `parentPath` | string | No | null | Parent by hierarchy path |

**Returns**: `{success, name, instanceId, hasUxml, hasPanelSettings, sortOrder}`

---

### uitk_set_document
Modify UIDocument properties on an existing scene GameObject. Adds UIDocument if not present.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | Find by GameObject name |
| `instanceId` | int | No* | 0 | Find by instance ID |
| `path` | string | No* | null | Find by hierarchy path |
| `uxmlPath` | string | No | — | New VisualTreeAsset path |
| `panelSettingsPath` | string | No | — | New PanelSettings path |
| `sortOrder` | int | No | — | New sort order |

*At least one of `name`/`instanceId`/`path` required.

**Returns**: `{success, name, instanceId, visualTreeAsset, panelSettings, sortingOrder}`

---

### uitk_create_panel_settings
Create a `PanelSettings` ScriptableObject asset.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `savePath` | string | Yes | — | Asset path, e.g. `Assets/UI/Panel.asset` |
| `scaleMode` | string | No | `"ScaleWithScreenSize"` | `ConstantPixelSize`, `ConstantPhysicalSize`, `ScaleWithScreenSize` |
| `referenceResolutionX` | int | No | 1920 | Reference width (ScaleWithScreenSize) |
| `referenceResolutionY` | int | No | 1080 | Reference height (ScaleWithScreenSize) |
| `screenMatchMode` | string | No | `"MatchWidthOrHeight"` | `MatchWidthOrHeight`, `Shrink`, `Expand` |
| `themeStyleSheetPath` | string | No | null | ThemeStyleSheet asset path |

**Returns**: `{success, path, scaleMode, referenceResolution, screenMatchMode}`

---

### uitk_list_documents
List all UIDocument components in the active scene.

No parameters.

**Returns**: `{count, documents: [{name, instanceId, visualTreeAsset, panelSettings, sortingOrder, active}]}`

---

## Inspection Skills

### uitk_inspect_uxml
Parse a UXML file and return its element hierarchy as a tree.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `filePath` | string | Yes | — | UXML asset path |
| `depth` | int | No | 5 | Max traversal depth |

**Returns**: `{path, hierarchy: {tag, attributes, children[]}}`

---

## Template Skills

### uitk_create_from_template
Generate a paired UXML+USS from a built-in template. Files are named `{name}.uxml` and `{name}.uss` under `savePath`.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `template` | string | Yes | Template type (see below) |
| `savePath` | string | Yes | Target directory, e.g. `Assets/UI` |
| `name` | string | No | Base filename (defaults to template name) |

**Template types**:

| Template | Contents |
|----------|----------|
| `menu` | Full-screen menu with title, Play/Settings/Quit buttons |
| `hud` | Absolute-positioned HUD: minimap, score label, health bar |
| `dialog` | Modal dialog: title, message, OK/Cancel buttons |
| `settings` | Settings panel: Volume sliders, Toggle, DropdownField |
| `inventory` | 3×3 grid inventory with ScrollView |
| `list` | Scrollable item list |

**Returns**: `{success, template, ussPath, uxmlPath, name}`

---

## Batch Skills

### uitk_create_batch
Batch create multiple USS and UXML files in one call.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `items` | string | Yes | JSON array of file descriptors |

**Item schema**:
```json
{
  "type": "uss",        // "uss" or "uxml"
  "savePath": "Assets/UI/Menu.uss",
  "content": "...",     // optional: file content
  "ussPath": "..."      // optional: for uxml, USS to reference
}
```

**Returns**: `{success, totalItems, successCount, failCount, results[]}`

---

## USS Properties Quick Reference

### Flex Layout
```css
.element {
    display: flex;
    flex-direction: row;          /* row | column */
    flex-wrap: wrap;              /* nowrap | wrap */
    flex-grow: 1;
    flex-shrink: 0;
    flex-basis: auto;
    align-items: center;          /* flex-start | flex-end | center | stretch */
    justify-content: space-between; /* flex-start | flex-end | center | space-between | space-around */
}
```

### Box Model
```css
.element {
    width: 200px;
    height: 100px;
    min-width: 50px;
    max-width: 500px;
    margin: 8px;                  /* or margin-top/right/bottom/left */
    padding: 16px;
    border-width: 1px;
    border-color: #333;
    border-radius: 4px;
}
```

### Text
```css
.element {
    font-size: 16px;
    color: #E0E0E0;
    -unity-font-style: bold;      /* normal | bold | italic | bold-and-italic */
    -unity-text-align: upper-left; /* upper-left | upper-center | middle-left | middle-center ... */
    white-space: normal;           /* nowrap | normal */
}
```

### Background & Border
```css
.element {
    background-color: #2D2D2D;
    background-color: rgba(0,0,0,0.5);
    border-color: #4A90D9;
    border-width: 1px;
    border-radius: 8px;
    -unity-background-scale-mode: stretch-to-fill;
}
```

### Positioning
```css
.element {
    position: absolute;           /* absolute | relative */
    top: 10px;
    left: 20px;
    right: 10px;
    bottom: 0;
    translate: 50% 0;
}
```

### Pseudo-classes
```css
.btn:hover   { background-color: #555; }
.btn:active  { background-color: #333; }
.btn:focus   { border-color: #4A90D9; }
.btn:checked { background-color: #4A90D9; }
.btn:disabled { opacity: 0.4; }
```

### CSS Variables (Custom Properties)
```css
:root {
    --primary: #4A90D9;
    --bg: #1E1E1E;
}
.element { color: var(--primary); background-color: var(--bg); }
```

---

## UXML Elements Quick Reference

### Layout Containers
```xml
<engine:VisualElement name="root" class="my-class" />
<engine:ScrollView mode="Vertical" name="scroll" />
<engine:GroupBox label="Section Title" />
<engine:Foldout text="Advanced" value="false" />
<engine:TwoPaneSplitView />
```

### Text & Labels
```xml
<engine:Label text="Hello World" name="my-label" />
<engine:TextField label="Name:" value="default" name="input" />
<engine:TextField multiline="true" />
```

### Buttons & Toggle
```xml
<engine:Button text="Click Me" name="btn" />
<engine:Toggle label="Enable" value="true" name="toggle" />
<engine:RadioButton label="Option A" value="true" />
<engine:RadioButtonGroup label="Choose:">
    <engine:RadioButton label="A" />
    <engine:RadioButton label="B" />
</engine:RadioButtonGroup>
```

### Sliders & Progress
```xml
<engine:Slider label="Volume" low-value="0" high-value="1" value="0.8" name="slider" />
<engine:SliderInt label="Count" low-value="1" high-value="10" value="5" />
<engine:ProgressBar title="Loading..." value="0.5" />
<engine:MinMaxSlider min-value="0" max-value="100" low-limit="0" high-limit="100" />
```

### Dropdowns & Lists
```xml
<engine:DropdownField label="Quality" choices="Low,Medium,High" value="Medium" name="dd" />
<engine:ListView name="list-view" />
<engine:TreeView name="tree-view" />
```

### Numeric Fields
```xml
<engine:IntegerField label="Count" value="0" name="count" />
<engine:FloatField label="Speed" value="1.5" />
<engine:LongField label="ID" value="0" />
<engine:Vector2Field label="Position" />
<engine:Vector3Field label="Position" />
<engine:RectField label="Bounds" />
<engine:ColorField label="Color" value="#FF0000FF" />
```

### Style Reference
```xml
<!-- Inline style reference (preferred for project paths) -->
<Style src="Assets/UI/MyStyle.uss" />
<!-- Package relative -->
<Style src="project://database/Assets/UI/MyStyle.uss" />
```

---

## End-to-End Example

```python
import unity_skills

# Step 1: Create PanelSettings
unity_skills.call_skill("uitk_create_panel_settings",
    savePath="Assets/UI/GamePanel.asset",
    scaleMode="ScaleWithScreenSize",
    referenceResolutionX=1920,
    referenceResolutionY=1080
)

# Step 2: Create USS stylesheet
unity_skills.call_skill("uitk_create_uss",
    savePath="Assets/UI/Game.uss",
    content="""
:root { --accent: #4A90D9; }
.container { width: 100%; height: 100%; align-items: center; justify-content: center; }
.title { font-size: 36px; color: white; -unity-font-style: bold; }
"""
)

# Step 3: Create UXML layout (references the USS)
unity_skills.call_skill("uitk_create_uxml",
    savePath="Assets/UI/Game.uxml",
    ussPath="Assets/UI/Game.uss",
    content="""<?xml version="1.0" encoding="utf-8"?>
<engine:UXML xmlns:engine="UnityEngine.UIElements">
    <Style src="Assets/UI/Game.uss" />
    <engine:VisualElement class="container">
        <engine:Label class="title" text="Game UI" name="title" />
        <engine:Button text="Start" name="btn-start" />
    </engine:VisualElement>
</engine:UXML>
"""
)

# Step 4: Create UIDocument in scene
unity_skills.call_skill("uitk_create_document",
    name="GameUI",
    uxmlPath="Assets/UI/Game.uxml",
    panelSettingsPath="Assets/UI/GamePanel.asset"
)

# Step 5: Use template shortcut instead
unity_skills.call_skill("uitk_create_from_template",
    template="menu",
    savePath="Assets/UI",
    name="MainMenu"
)
# → Creates Assets/UI/MainMenu.uss + Assets/UI/MainMenu.uxml

# Step 6: Batch create files
unity_skills.call_skill("uitk_create_batch", items='''[
    {"type": "uss", "savePath": "Assets/UI/HUD.uss"},
    {"type": "uxml", "savePath": "Assets/UI/HUD.uxml", "ussPath": "Assets/UI/HUD.uss"}
]''')

# Step 7: Read and edit file
result = unity_skills.call_skill("uitk_read_file", filePath="Assets/UI/Game.uss")
new_content = result["result"]["content"].replace("#4A90D9", "#E94560")
unity_skills.call_skill("uitk_write_file",
    filePath="Assets/UI/Game.uss",
    content=new_content
)

# Step 8: Inspect UXML structure
unity_skills.call_skill("uitk_inspect_uxml",
    filePath="Assets/UI/Game.uxml",
    depth=3
)

# Step 9: List all UIDocuments in scene
unity_skills.call_skill("uitk_list_documents")
```

---

## Workflow Notes

1. **File → Scene**: Create USS+UXML first, then assign to UIDocument in scene
2. **PanelSettings required**: Without PanelSettings, UIDocument won't render at runtime
3. **Style src paths**: Use project-relative paths starting with `Assets/` in `<Style src="..."/>`
4. **Read-Modify-Write**: Use `uitk_read_file` → edit content → `uitk_write_file` for incremental changes
5. **Batch for efficiency**: Use `uitk_create_batch` when creating 2+ files to avoid repeated API calls
