# Compilador

En el compilador actualmente se realiza el analisis lexico para un compilador, donde este se conforma de un clase automotaCompilador el cual realiza los siguientes pasos

Entrada > Filtrado > Reconocimiento > Respuesta

``` c#
    automotaCompilador automata = new automotaCompilador();
```

## Entrada
La entrada simplmente recibe una cadena de caracteres desde el metodo 'evaluate()' el cual nos permite meter la cadena y empezar el proceso de el automata.
``` c#
    automotaCompilador.evalute(input: string);
```
## Filtrado
En el filtrado lo que se realiza es leer la cadena y filtrarla entre los posibles divisores de palabras en el lenguaje, desde operadores, espacios o comillas para asi separar las cadenas en cadenas que se pueden comparar para verificar que es, esto agregando cada una de las cadenas filtradas a una Queue donde van a ser reconocidas

## Reconocimiento

En el reconocimiento se va realizando un Dequeue cadena por cadena donde en un filtrador realizado entre expreciones regulares y listas de palabras reservadas se va determinando que tipo es cada cadena agregandolo al mensaje de salida la cadena y el tipo determinado que se encontro.

## Mensaje

El mensaje es una propiedad publica de el automata, que unicamente se puede leer, esta propiedad obtiene un valor una vez terminando el metodo evaluate, siendo el mensaje de cada una de las cadenas evaluadas con el respectivo signo con el que fue encontrado.

``` c#
    automotaCompilador.msg;
```

# Analizador Semantico


# Analizador Sintactico