# Morph Merger for VaM

Combine your look into one morph by merging your morphs.

Share a character without needing to share all those morphs, receive a look / scene without having to worry about bad morphs.

Create a 'base morph' for values that make up a look, only morphs needed for animation then need to be separate.

![Plugin screenshot](https://raw.githubusercontent.com/ProjectCanyon/morph-merger/releases/release-1-0-5/MorphMerger105.png)

## Instructions

* Add plugin to the Person.
* Open the Plugin UI as per usual.
* Select the morphs you would like to merge on the right, it filters to only show you morphs that have been set from their default, it also hides invisible morphs.
Morphs only slightly used will be white, morphs either set to their min or max extent will be solid green.
It will show morphs used by blinking and auto behaviors so you might want to turn that off, or make sure you deselect them.
If you have genital morphs it will create a separate morph for them so that it can be in the correct morph folder.
* Give your morph a name to make it unique using text field supplied, optionally change your group and region name (Region will specify which category the morph is listed under).
* Hit the big red Merge button and all being well it will place the morphs in the correct base folders.
* You won't be able to see your new morph until either you restart VaM or you do a hard reset from the main menu.
* Your new morph should show up as morphs under _MorphMerger or the region name you specified.


## Version History

**7th July 2019: Updated to v1.0.3**
* Updated to include some tools to reset selected morphs to default, this also fixes the issue that once a morph's value is changed it will be included in exports. So no more morphs exported in .VAC files for morphs not in use. This is actually caused by the way VaM tracks modified morphs in that it does not clear down the selectableMorph array for defaulted morphs, previously you'd need to reset the scene. The code is included in the ' SetBankToDefault' method if interested.

**29th July 2019: Updated to v1.0.4**
* Thanks to DJ_clem for raising an issue around how it would take the atom's name as the morph name, this was because I didn't know how to do text input fields. Thanks to some code from Alazi on Discord I know how to do this now. I've therefore added 3 text fields, Morph Name, Morph Region & Morph Group. Name should be obvious and Region is where the morph will appear in the morphs category dropdown. * WARNING: It will overwrite the morph based on the name you give, so If you use the name of a morph you've used before it will overwrite. I can't really check this easily as for security reasons access to System.IO is locked down.

**28th Sept 2019: Updated to v.1.0.5**
* Includes fix for 1.18.04, the breaking change was the removal of morphNames property, this is only actually used in the code to reset a morph to default. As this is now available in vanilla VAM in 1.18 I've just removed that code. If people found that useful and I have some time I'll hunt down how Meshed is doing it now and copy it.
* By request changed the default sort order from alphabetical to 'by magnitude', i.e. the morphs having the most effect will be at the top. If people hate this I can switch it back, or better yet put a sort by option in when I have time.
* Also by request it will now add a 8 character random id to the end of the morph names to promote renaming to stop generic morph names flooding the community, overwriting and causing carnage!
