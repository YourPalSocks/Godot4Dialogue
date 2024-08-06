# 🗣️🗣️ Godot 4 Dialogue System 🗣️🗣️
A dynamic dialogue system for Godot 4.x .
## Features:
- Read text chunks from [YAML](https://yaml.org/) files
- Different typing speeds
- Events to allow player choices
- Custom signals for certain events
- Actions tied to Godot Input Map for easy re-mapping
- Create your own events to trigger functionality on certain lines 
  
## (Current) Limitations:
- Check out the [Issues](https://github.com/YourPalSocks/Godot4Dialogue/issues) page for more information.

# Example File
A simple dialogue with 3 lines, ending in a choice:
```yaml
test:
  text: 
    - 'Me:This is text'
    - 'Me:Test text'
    - 'Text?'
  options:
    event:
      type: 'Choice'
      name: 'test_choice'

test_choice:
  choices:
    - 'Option 1'
    - 'Option 2'
    - 'Option 3'
  results:
    - 'test_choice_result_a'
    - 'test_choice_result_b'
    - 'test_choice_result_c'

test_choice_result_a:
  text:
    - 'You:Result 1-1'
  options:
    event:
      type: 'Choice'
      name: 'test_choice2'

test_choice_result_b:
  text:
    - 'You:Result 1-2'
  options:
    event:
      type: 'Transition'
      name: 'ending_text'

test_choice_result_c:
  text:
    - 'You:Result 1-3'
  options:
    event:
      type: 'Transition'
      name: 'ending_text'

test_choice2:
  choices:
    - 'Subchoice 1'
    - 'Subchoice 2'
  results:
    - 'subchoice_result_a'
    - 'subchoice_result_b'

subchoice_result_a:
  text:
    - 'You:Result 2-1'
  options:
    event:
      type: 'Transition'
      name: 'ending_text'

subchoice_result_b:
  text:
    - 'You:Result 2-2'
  options:
    event:
      type: 'Transition'
      name: 'ending_text'

ending_text:
  text:
    - 'The end...'
```
## Chunking
The chunks `test` and `test_choice` represent two dialogue objects: `test` is a straightforward set of lines to be printed, and `test_choice` is a choice prompt with 3 different options, and 3 different reponses.

Additionally, `test_choice_result_a` will launch another choice event: `test_choice2`

All paths of this dialogue will converge with the `ending_text` chunk being played last, regardless of what path was taken to get there.