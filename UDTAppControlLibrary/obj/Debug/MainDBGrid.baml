   M S B A M L   `   `   `  ����\  XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35`Dclr-namespace:System.Windows.Controls;assembly=PresentationFrameworkSystem.Windows.Controls  5	      O KUDTAppControlLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullO,clr-namespace:UDTAppControlLibrary.BehaviourUDTAppControlLibrary.Behaviour 5
      Q-clr-namespace:UDTAppControlLibrary.ValidationUDTAppControlLibrary.Validation 5      c _System.Windows.Interactivity, Version=4.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity 5      %   UDTAppControlLibrary.MainDBGrid   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml      5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006      5      @d2http://schemas.microsoft.com/expression/blend/2008      5      P LPrism.Wpf, Version=6.3.0.0, Culture=neutral, PublicKeyToken=40ee6c3a2184dc59$prismhttp://prismlibrary.com/  5      VsysControlsDclr-namespace:System.Windows.Controls;assembly=PresentationFramework  5	      8local,clr-namespace:UDTAppControlLibrary.Behaviour  5
      9rules-clr-namespace:UDTAppControlLibrary.Validation  5      XiPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity  5      !  Prism.Mvvm.ViewModelLocator    AutoWireViewModel  .  � 	Resources 5          BooleanToVisibilityConverter  validationErrorTemplate �� Path ��Name��%�   &	        &	      �� 5   
   63   �� 5   
   .R�5      ]� .+�5      �� 
���  ��$	��Top=�5      ��5   -   �� $ [0].ErrorContent�5   $   �� 5      $ ErrorAdorner��5      5      5      5      5      .��5      � $��-10,0,-217,-184q�6   $
��Left=�6$   $	��527��6?    J� RenderTransformOrigin$ 
0.51,0.506&�6K   .+�5    
   &    System.Windows.Controls.DataGrid     
IsReadOnly .    CanUserAddRows .  $
��Left=�5&      $��	32,38,0,0q�6.   $	��Top=�6A   $	��224��6Y   $	��181��6e      AutoGenerateColumns 5       �� *AutoColumns+��5"      �� *
DataSets+ �� Mode$ TwoWay=�	 �� SelectedIndex	 5#      �� *SelectedIndex+
 �� SelectedItem
 5$      �� *SelectedItem+$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�P  JUDTAppControlLibrary.Behaviour.AutoGeneratingColumnEventToCommandBehaviour   Command 5%      �� *CreateColumnsCommand+.  (System.Windows.Interactivity.Interaction   Triggers 5'      /  )System.Windows.Interactivity.EventTrigger  5(         	EventName$ AutoGeneratingColumn��6!   .  (System.Windows.Interactivity.TriggerBase   Actions. 5)      6  0System.Windows.Interactivity.InvokeCommandAction     Command 6,   �� *AutoGeneratingColumnCommand+5*      5,      � 5.   
   y�6   �� *IsMasterVisible+ �� 	Converter# [  .+�5/      �� $��Add Data Set��64   $
��Left=�6K   $��
270,62,0,0q�6f   $	��Top=�6z   $	��102��6�   ��6   �� *
AddCommand+6�   �� 50      $��Delete Data Set��67   $
��Left=�6Q   $��
270,92,0,0q�6l   $	��Top=�6�   $	��102��6�   ��6   �� *DeleteCommand+6�   �� 51      $
��Left=�6   $��23��61   $��345,130,0,0q�6=    �� TextWrapping$
 Wrap=�6R    �� 
IsReadOnly .  $	��Top=�55      $	��120��6.   ��52      �� *Name+$ TwoWay=�$ PropertyChanged=�{�53      �� *IsInputEnabled+55   .   �� 56      3  -UDTAppControlLibrary.Behaviour.InputBehaviour   IsAlphaOnly . $
��Left=�6>   $��23��6Y   $��345,162,0,0q�6e   $
 Wrap=�6z   $	��Top=�59      $	��143��6.   ��57      �� *Description+$ TwoWay=�$ PropertyChanged=�{�58      �� *IsInputEnabled+59   .   �� 5:      $
��Name��6   $
��Left=�6#   $��298,127,0,0q�6>   $	��Top=�6S   �� 5;      $��Description��6   $
��Left=�6*   $��270,158,0,0q�6E   $	��Top=�6Z   �� 5<      $
��Save��65   $
��Left=�6D   $��
390,62,0,0q�6_   $	��Top=�6s   $��75��6�   ��6   �� *SaveCommand+6�   �� 5=      $��Cancel��67   $
��Left=�6H   $��
390,92,0,0q�6c   $	��Top=�6w   $��75��6�   ��6   �� *CancelCommand+6�   5>        5?   
   $
��Left=�6   $��
32,252,0,0q�6.   $	��Top=�6B   $	��144��6Z   $	��224��6g    . ��5@      �� *	DataItems+	 5A      �� *ChildSelectedIndex+
 5B      �� *ChildSelectedItem+$ TwoWay=�$ PropertyChanged=�5C      � 5D   
   y�6   �� *IsDetailVisible+# [  .+�5E      �� $
��Save��65   $
��Left=�6D   $��413,252,0,0q�6_   $	��Top=�6t   $��75��6�   ��6   �� *SaveCommand+6�   �� 5F      $��Delete Data Item��67   $
��Left=�6R   $��282,280,0,0q�6m   $	��Top=�6�   $	��108��6�   ��6   �� *DeleteCommand+6�   �� 5G      $��Cancel��67   $
��Left=�6H   $��413,280,0,0q�6c   $	��Top=�6x   $��75��6�   $ 
0.87,0.602&�6�   ��6   �� *CancelCommand+6�   �� 5H      $��Add Data Item��64   $
��Left=�6L   $��282,252,0,0q�6g   $	��Top=�6|   $	��108��6�   ��6   �� *
AddCommand+6�   � 5I      y�6   �� *IsItemVisible+# [  .+�5J      �� $
��Left=�5M      $��23��60   $��359,320,0,0q�6<   $
 Wrap=�6Q   $	��Top=�6e   $	��129��6}   ��5K      �� *	ChildName+$ TwoWay=�$ PropertyChanged=�{�5L      �� *IsInputEnabled+5M   }   �� 5N      $
��Left=�5Q      $��23��60   $��359,360,0,0q�6<   $
 Wrap=�6Q   $	��Top=�6e   $	��129��6}   ��5O      �� *Type+$ TwoWay=�$ PropertyChanged=�{�5P      �� *IsInputEnabled+5Q   }   �� 5R      $
��Name��6   $��Right=�6'   $
��Left=�6J   $��318,320,0,0q�6e   $	��Top=�6z   $��41��6�   $ 0.293,0.628&�6�   �� 5S      $
��Type��6   $��Right=�6'   $
��Left=�6J   $��313,360,0,0q�6e   $	��Top=�6z   $��41��6�   $ 0.997,0.569&�6�   $��23��6�   5T      � 5U      y�6   �� *IsRelationVisible+# [  .+�5V      �� {�.  $
��Left=�5Y      $��23��60   $��359,320,0,0q�6<   $
 Wrap=�6Q   $	��Top=�6e   $	��129��6}   ��5W      �� *ParentDataSet+$ OneWay=�$ PropertyChanged=�5Y   }   �� 5[       =� ErrorTemplate# [ 5]      $
��Left=�5^      $��23��60   $��359,360,0,0q�6<   $	��Top=�6Q   $	��129��6i   ��5[      �� *ChildOptions+	 5\      �� *
ComboIndex+$ TwoWay=�
 5_      �� 5`      $ SelectedChild�6"   $ TwoWay=�67   $ PropertyChanged=�6E   5d      ��5f      �� 5g      .R�5h      ��  �� Orientation$ 
Horizontal=�6)   .+�5i   "   �� ��6,   �� 5j      5k      5m      �� 5n      $��Parent Dataset��6   $��Right=�61   $
��Left=�6T   $��274,320,0,0q�6o   $	��Top=�6�   $
��Auto��6�   $ 0.293,0.628&�6�   �� 5o      $��Child Dataset��6   $��Right=�60   $
��Left=�6S   $��274,360,0,0q�6n   $	��Top=�6�   $
��Auto��6�   $ 0.997,0.569&�6�   $��23��6�   5p      5q      5r      5s      