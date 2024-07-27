# 🗣️🗣️ Godot 4 Dialogue System 🗣️🗣️
A dynamic dialogue system for Godot 4.x .
## Features:
- Read text chunks from [YAML](https://yaml.org/) files
- Different typing speeds
- Events to allow player choices
- Custom signals for dialogue closing
- Actions tied to Godot Input Map for easy re-mapping
- Create your own events to trigger functionality on certain lines 
  
## (Current) Limitations:
- Events cannot launch off of each other
- Choices can only add 1 line of dialogue
- A piece of dialogue can only have one 'speaker'
- Check out the [Issues](https://github.com/YourPalSocks/Godot4Dialogue/issues) page for more information.

# Example File
A simple dialogue with 3 lines, ending in a choice:
```yaml
test:
  text: 
    - 'This is text'
    - 'Test text'
    - 'Text?'
  options:
    event:
      line: 3
      type: 'Choice'
      name: 'test_choice'

test_choice:
  choices:
    - 'Option 1'
    - 'Option 2'
    - 'Option 3'
  results:
    - 'Result 1'
    - 'Result 2'
    - 'Result 3'

```
## Chunking
The chunks `test` and `test_choice` represent two dialogue objects: `test` is a straightforward set of lines to be printed, and `test_choice` is a choice prompt with 3 different options, and 3 different reponses.