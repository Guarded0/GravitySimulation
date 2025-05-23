# Projet de SSI
Une simulation à N-corps avec génération procédurale de planètes et le rendu de l'atmosphère et des océans.
![image](https://github.com/Guarded0/GravitySimulation/blob/main/Images/mainImage.png)
# Caractéristiques :
 - Planètes et étoiles entièrement personnalisables et procédurales créées et rendues presque entièrement par le GPU.
 - Simulation fonctionnelle et personnalisable de N-corps complètement indépendante du moteur physique de unity.
 - Prédiction d'orbite calculer en simulant les mêmes corps à vitesse beaucoup plus rapide.
 - La possibilité de changer le cadre de référence pour n'importe quel corps céleste ou aucun.
 - La possibilité d'éditer toutes les propriétés de chaque corps céleste pour que l'utilisateur puisse créer un système solaire à sa convenance.
 - Une fonction de sauvegarde et de chargement permettant à l'utilisateur de stocker et éventuellement de partager ses créations.
 - La possibilité de sauvegarder et d'utiliser des préréglages de planètes que l'utilisateur souhaite conserver.
 - Une interface utilisateur claire et intuitive pour faciliter au maximum l'expérience de l'utilisateur dans la création de son système.

# Guide d'utilisation : 
Lors du lancement de l'application, vous pourrez voir un system composer d'un soleil et de deux planète.
Vous pouvez vous déplacer entre les corps céleste horizontalement grace à w,a,s,d et verticalement grace à q et e. 
Le click droit de la souris vous permets de modifier l'angle de la caméra. 

Le menu "Paramètre" à droite de l'écran permets d'ajuster les paramètre de simulation simulation suivant: 
- Le slider "Vitesse de simulation" permet d'ajuster la vitesse à laquelle la simulation effectue ces update. 
- Le slider "Constant gravitationnelle" permet d'ajuster la valeur de cette constante dans la lois de la gravitation de Newton
- la case à cocher "Montrer orbit" permets d'afficher l'orbite de chaque corps celeste de votre systême.

Par la suite vous pouvez utiliser le click gauche de votre souris pour sélection un cible.
Lorsque vous visez une cible, vos déplacement horizontale devient un rotation atour du courps célesete tandis que vos déplacement vertical vous permette maintenant de vous éloigner et de vous approcher du corps céleste.
Des flèche vont appaitre sur votre cible en cliquer avec le click gauche de votre souris sur ces flêche vous pourrez déplacer  le corps céleste dans la direction de la flêche.
Lorsque vous visez un cible le menu "Inspecteur apparait. Il vous permets de modifier l'apparence ansi que les caractéristique physique du corps céleste selectioner.
La boite de text ansi que le bouton "+" au bas de ce menu vous permet de sauvergarder les caracteristique du corps céleste que vous venez de modifier sous la forme d'un bouton.
Ces Bouton seras positioner a l'interieur du "Menu de Construction".

Le "Menu de construction" est séparer en deux menu, le menue planète, et le menue étoile. Des bouton qui vous permette de création de novuelle planète grâce a une mécanique de "Drag and Drop" sont automatiquement disposer dans l'un ou l'autre de ces menu. Vous pouvez changer le menu qui est visble grace au bouton |Planete| et au bouton |Étoile|. 
Vous pouvez utiliser le bouton "x" présent sur les bouton du "Menu de construction" pour suprimer un bouton. 
Lorsque vous clicker sur un des bouton de ces menu, une sphère verte de la taille du corps céleste que vous allez crée vas apparaitre. 
Cette sphère vous permets de visualition ou vas apparaitre votre prochain corps céleste. 
Vous pouvez annuler la création d'un planète a tout moment en appuyant sur le click droit de votre souris. 
Vous pouvez confirmer le positionement de votre planète en appuyant sur le click gauche de votre souris 
Par la suit, vous devez sélectioner la vitesse à laquelle votre corps céleste seras projeter dans votre système en déplacant votre souris. 
Lorsque vous avez choisit votre vitesse de projection vous pouvons lancer votre corps céleste grâce au click gauche de votre souris. 

Le "Menu paramètre" et le "Menu Construction" on un bouton "<" ou "v" qui vous permêt de cacher ou de faire réapparaitre ces menu. 

Dans le coins supérieux gauche de votre écrant il y a un bouton hambergeur qui vous permets d'afficher le Menu principale. 
Le bouton "Sauvegarder" sauvegard le système qui est présentement charger dans le logiciel. 
Le bouton "Charger" vous permets de charge un sauvegarde d'un système. 
Le bouton "paramètre" affiche un menu ou vous pourrez modifier la résolution, mêtre le logiciel en pleins écrans, afficher les fps et modifier la sensibilité de votre camera. 
Le bouton "Quitter" ferme l'application.
**Fermer l'application sans sauvegarde annuleras les modification que vous avez effectuer sur votre système planètaire**

Dans le coins supérieux droit de votre écrant, il y a un bouton quadrilier qui vous permet d'afficher un grille pour faciliter le positionement des corps céleste. 

# Technologie utiliser : 
 - Motheur de Jeux: Unity
 - Language: C# et HLSL
 - Librairie: Lean Tween 
