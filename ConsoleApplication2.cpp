#include <iostream>

using namespace std;

// Realizar la suma


namespace complejas{
	constexpr auto e = 2.71828182845904523536f;
	constexpr auto pi = 3.14159265358979323846f;
	constexpr auto radpi = 180.f / pi;
	constexpr auto sqrpi = pi * pi;


	double cosh(double x) {
		__asm {
			fld		qword ptr[esp + 4]
			fldl2e
			fmulp	st(1), st
			fld1
			fld		st(1)
			fprem
			f2xm1
			faddp	st(1), st
			fscale
			fxch
			fstp	st
			fld1
			fdiv	st, st(1)
			faddp	st(1), st
			mov		eax, 0x3F000000
			push	eax
			fld		dword ptr[esp]
			fmulp	st(1), st
			pop		eax
			ret		8
		}
	}
	double tanh(double x) {
		__asm {
			fld		qword ptr[esp + 4]
			fld		st
			mov		eax, 0x40000000
			push	eax
			fld		dword ptr[esp]
			fmul	st, st(1)
			fldl2e
			fmulp	st(1), st
			fld1
			fld		st(1)
			fprem
			f2xm1
			faddp	st(1), st
			fscale
			fxch
			fstp	st
			fld1
			fsub	st, st(1)
			fchs
			fld1
			faddp	st(2), st
			fdivrp	st(1), st
			pop		eax
			ret		8
		}
	}
	double sinh(double x) {
		__asm {
			fld		qword ptr[esp + 4]
			fldl2e
			fmulp	st(1), st
			fld1
			fld		st(1)
			fprem
			f2xm1
			faddp	st(1), st
			fscale
			fxch
			fstp	st
			fld1
			fdiv	st, st(1)
			fsubp	st(1), st
			mov		eax, 0x3F000000
			push	eax
			fld		dword ptr[esp]
			fmulp	st(1), st
			pop		eax
			ret		8
		}
	}
	double pow(double x, double y) {
		__asm {
			fld		qword ptr x
			fld		qword ptr y
			ftst
			fstsw	ax
			sahf
			jz		skip

			fyl2x
			fld1
			fld		st(1)
			fprem
			f2xm1
			faddp	st(1), st(0)
			fscale

			skip :

				fstp	st(1)
				ret	16
		}

	}
	double sqrt(double x) {
		__asm {
			fld		qword ptr[esp + 4]
			fsqrt
			ret		8
		}
	}

}
int suma(int $a, int $b)
{
	int $result;
	__asm{
	mov eax, $a;
	mov ebx, $b;
	add ebx, eax;
	mov $result, ebx;
	};
	return $result;
}

double coseno(double $a)
{
	$a = $a * 3.14 / 180;
	double $result;
	__asm {
		fld qword ptr $a;
		fcos;
		fstp qword ptr $result;
	};
	return $result;
}

double cosenoh(double $a)
{
	
	double $result;
	__asm {
		fld qword ptr[$a]; 
		fldl2e
		fmulp		st(1), st
		fld1
		fld		st(1)
		fprem
		f2xm1
		faddp		st(1), st
		fscale
		fxch
		fstp		st
		fld1
		fdiv		st, st(1)
		fsubp		st(1), st
		mov		eax, 0x3F000000
		push		eax
		fld		dword ptr[esp]
		fmulp		st(1), st
		pop		$result
		// ST1 ST0 $result
	}
	return $result;
}



double senoh(double $a)
{
	
	double $result;
	__asm {
		fld qword ptr $a;
		fsin;
		fld1            // Cargar el valor 1 en la pila
		fadd            // Sumar 1 y el resultado del seno
		fld1            // Cargar el valor 1 en la pila
		fdiv            // Dividir la suma anterior por 2
		fstp qword ptr $result;
	};
	return $result;
}

double seno(double $a)
{

	double $result;
	__asm {
		fld qword ptr $a;
		fsin;
		fstp qword ptr $result;
	};
	return $result;
}

double tangente(double $a)
{

	double $result;
	__asm {
		fld QWORD PTR $a 
		fptan 
		fstp ST(0) 
		fstp QWORD PTR $result 

	};	
	return $result;
}

double tangenteh(double $a)
{

	double $result;
	__asm {
		fld QWORD PTR $a
		fptan
		fstp ST(0)      // Sacar el resultado de la pila y descartar el valor "1" que se obtiene de la operación "fptan"
		fstp QWORD PTR $result

	};
	return $result;
}

// Realizar una resta
int resta(int $a, int $b)
{
	int $result;
	__asm {
	mov eax, $a;
	mov ebx, $b;
	sub eax, ebx;
	mov $result, eax;
	}
	return $result;
}
//Minimo comun divisor
int gcd(int $a, int $b)
{
	int $result;
	_asm{
	mov eax, $a;
	mov ebx, $b;
	CONTD: cmp ebx, 0;
	je DONE;
	xor edx, edx;
	idiv ebx;
	mov eax, ebx;
	mov ebx, edx;
	jmp CONTD;
	DONE: mov $result, eax;
	}
	return $result;
}
// realizar la resta
int sub(int x, int y) {
	return x - y;
}


// realizar la multiplicacion con 3 factores
double mul(double a, double b) {
	double resultado;
	__asm{
		fld qword ptr a; carga el valor de a en ST0
		fld qword ptr b ; carga el valor de b en ST1
		fmul ST(0), ST(1); multiplica ST0 por ST1 y guarda el resultado en ST0
		fstp qword ptr resultado ; guarda el resultado en la variable c
	// lista clobber vacia
	};
	return resultado;
}
double div(double a, double b) {
	double resultado;
	__asm {
		fld qword ptr a; carga el valor de a en ST0
		fld qword ptr b; carga el valor de b en ST1
		fdiv ST(0), ST(1); multiplica ST0 por ST1 y guarda el resultado en ST0
		fstp qword ptr resultado; guarda el resultado en la variable c
		// lista clobber vacia
	};
	return resultado;
}
int main() {
	int i;
	double x, y;
	cout << "Que operacion desea realizar?" << endl;
	cout << "1. Suma" << endl;
	cout << "2. Resta" << endl;
	cout << "3. Multiplicacion" << endl;
	cout << "4. Division" << endl;
	cout << "5. Coseno" << endl;
	cout << "6. Seno" << endl;
	cout << "7. Tangente" << endl;
	cout << "8. Coseno Hiperbolico" << endl;
	cout << "9. Seno Hiperbolico" << endl;
	cout << "10 Tangente Hiperbolica" << endl;
	cout << "11 Potencia" << endl;
	cout << "12 Raiz Cuadrada" << endl;
	cout << ">> ";
	cin >> i;
	switch (i)
	{
	case 1:  
			cout << "Ingrese el primer operando" << endl;
			cin >> x;
			cout << "Ingrese el segundo operando" << endl;
			cin >> y;
			cout << "La suma de" << x << " y " << y << " es:" << suma(x, y);

		break;
		case 2: 
			cout << "Ingrese el primer operando" << endl;
			cin >> x;
			cout << "Ingrese el segundo operando" << endl;
			cin >> y;
			cout << "La resta de" << x << " y " << y << " es:" << suma(x, -y);
			break;

		case 3:
			cout << "Ingrese el primer operando" << endl;
			cin >> x;
			cout << "Ingrese el segundo operando" << endl;
			cin >> y;
			cout << "La multiplicacion de" << x << " y " << y << " es:" << mul(x, y);

			break;

		case 4: 
			cout << "Ingrese el primer operando" << endl;
			cin >> x;
			cout << "Ingrese el segundo operando" << endl;
			cin >> y;
			cout << "La Division de" << x << " y " << y << " es:" << div(x, y);

			break;
		case 5: 
			cout << "Ingrese el angulo" << endl;
			cin >> x;
			cout << "Cos(" << x << " ) " << " = " << coseno(x);

			break;
		case 6: 

			cout << "Ingrese el angulo" << endl;
			cin >> x;
			cout << "Sen(" << x << " ) " << " = " << seno(x);
			break;

		case 7: 
			cout << "Ingrese el angulo" << endl;
			cin >> x;
			cout << "Tan(" << x << " ) " << " = " << tan(x);
			break;
		case 8: 
			cout << "Ingrese el angulo" << endl;
			cin >> x;
			cout << "Cosh(" << x << " ) " << " = " << cosh(x);
			break;
		case 9: 
			cout << "Ingrese el angulo" << endl;
			cin >> x;
			cout << "Senh(" << x << " ) " << " = " << sinh(x);
			break;
		case 10: 
			cout << "Ingrese el angulo" << endl;
			cin >> x;
			cout << "Tanh(" << x << " ) " << " = " << tanh(x);
			break;

		case 11:
			cout << "Ingrese la base" << endl;
			cin >> x;
			cout << "Ingrese potencia" << endl;
			cin >> y;
			cout << "La Potencia de" << x << " elevado a " << y << " es:" << pow(x, y);

			break;
		case 12: 
			cout << "Ingrese el operandoo" << endl;
			cin >> x;
			cout << "Raiz(" << x << " ) " << " = " << sqrt(x);
			break;
	default:
		break;
	}
	cout << endl;
	system("pause");
	system("cls");
	main();
}