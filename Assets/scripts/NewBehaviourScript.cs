using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class NewBehaviourScript : MonoBehaviour {

    public static System.Random rand = new System.Random();

    private Rigidbody2D rigid;
    private ArrayList ceszki, primitiveCeszki;
    // stałe maszynki poruszania
    private Vector2 minDrag;

    // zmienne maszynki poruszania
    private Vector2 mov;
    private Vector2 drag;

    private Vector2 scl;

    // zmienne pomocnicze
    private Vector2 dragForce;
    private Shocker dragShocker;
    private DoubleModuler movDoubleModulerX, movDoubleModulerY;

    // lista najbardziej prymitywnych cech
    Ceszka MaxVelocity = new Ceszka(100);
    Ceszka Drag = new Ceszka(14);
    Ceszka DragShocker = new Ceszka(0);
    Ceszka MovDoubleModulerX = new Ceszka(0);
    Ceszka MovDoubleModulerY = new Ceszka(0);

    // lista ceszek
    Ceszka ropiejaceStopy = new Ceszka(2);
    Ceszka alkoholizm = new Ceszka(3);

    private void loadCeszki()
    {
        ceszki = new ArrayList();
        ropiejaceStopy.add(DragShocker, (float x) => -x);
        ropiejaceStopy.add(MaxVelocity, (float x) => -15*x);
        ceszki.Add(ropiejaceStopy);
        alkoholizm.add(MovDoubleModulerX, (float x) => x);
        alkoholizm.add(MovDoubleModulerY, (float x) => x);
        ceszki.Add(alkoholizm);
    }

    private void loadPrimitiveCeszki()
    {
        primitiveCeszki = new ArrayList();
        primitiveCeszki.Add(MaxVelocity);
        primitiveCeszki.Add(Drag);
        primitiveCeszki.Add(DragShocker);
        primitiveCeszki.Add(MovDoubleModulerX);
        primitiveCeszki.Add(MovDoubleModulerY);
    }

    void Start()
    {
        mov = new Vector2();
        drag = new Vector2();
        minDrag = new Vector2(2, 2);
        scl = new Vector2();
        dragForce = new Vector2();
        dragShocker = new Shocker(1, 5);
        movDoubleModulerX = new DoubleModuler(0.3f, 10);
        movDoubleModulerY = new DoubleModuler(0.3f, 10);
        rigid = GetComponent<Rigidbody2D>();
        loadPrimitiveCeszki();
        loadCeszki();
    }

    void FixedUpdate()
    {

        // update ceszek
        // wyzerowanie prymitywnych ceszek
        foreach (Ceszka c in primitiveCeszki)
        {
            c.y = 0;
        }
        // wyliczenie nowych wartości
        foreach (Ceszka c in ceszki)
        {
            c.calculateValue();
        }

        //update zmiennych pomocniczych
        dragShocker.update(Time.deltaTime);
        movDoubleModulerX.update(Time.deltaTime);
        movDoubleModulerY.update(Time.deltaTime);

        // input
        mov.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // wpływ ceszek na zmienne maszynki poruszania
        if (MaxVelocity.getValue() > 1)
        {
            scl.Set(MaxVelocity.getValue(), MaxVelocity.getValue());
        }
        mov.Scale(scl);
        mov += new Vector2(MovDoubleModulerX.getValue() * movDoubleModulerX.value, MovDoubleModulerY.getValue() * movDoubleModulerY.value);

        drag.Set(0, 0);
        drag += new Vector2((float)Math.Pow(1.2f, Drag.getValue()), (float)Math.Pow(1.2f, Drag.getValue()));
        drag += new Vector2(dragShocker.value*DragShocker.getValue(), dragShocker.value*DragShocker.getValue());

        // poruszenie ziomkiem za pomocą zmodyfikowanych zmiennych maszynki poruszania
        rigid.AddForce(mov);
        dragForce.Set(-(minDrag.x + drag.x)*rigid.velocity.x, -(minDrag.y + drag.y)*rigid.velocity.y);
        rigid.AddForce(dragForce);
    }

    public delegate float Funkcja(float x);

    public class Ceszka
    {
        public float y, dx; // y - wartość   dx - przesunięcie
        ArrayList ceszki;
        List<Funkcja> funkcjeCeszek;

        public Ceszka(float dx)
        {
            this.dx = dx;
            ceszki = new ArrayList();
            funkcjeCeszek = new List<Funkcja>();
        }
        
        public void add(Ceszka ceszka, Funkcja funkcja)
        {
            ceszki.Add(ceszka);
            Funkcja fun = funkcja;
            funkcjeCeszek.Add(fun);
        }

        public float getValue()
        {
            return y + dx;
        }

        public void calculateValue()
        {
            int i = 0;
            foreach(Ceszka c in ceszki)
            {
                c.y += funkcjeCeszek[i](y + dx);
                c.calculateValue();
                i++;
            }
        }
    }

    // narzędzie do wywoływania gwałtownych zaburzeń
    public class Shocker
    {
        public float value;
        public float time, strength;
        private float deltaTime;

        public Shocker(float time, float strength)
        {
            this.time = time;
            this.strength = strength;
            value = 0;
            deltaTime = 0;
        }

        public void update(float delta)
        {
            deltaTime += delta;
            if (deltaTime >= time)
            {
                deltaTime -= time;
                value = rand.Next(0, 100)/100f * strength;
            }
        }
    }

    // narzędzie do wywoływania gładkich zaburzeń
    public class Moduler
    {
        public float value;
        public float strength;
        private float module, time;

        public Moduler(float time, float strength)
        {
            this.time = time;
            this.strength = strength;
            value = 0;
            module = 0;
        }

        public void update(float delta)
        {
            module += (rand.Next(0, 200)-100) / 100f * delta / time;
            if (module > 1) module = 1;
            else if (module < 0) module = 0;

            value = module * strength;
        }
    }

    // Moduler, tylko z dodanymi wartościami ujemnymi
    public class DoubleModuler
    {
        public float value;
        float strength;
        Moduler moduler;

        public DoubleModuler(float time, float strength)
        {
            this.strength = strength;
            moduler = new Moduler(time, 2*strength);
        }

        public void update(float delta)
        {
            moduler.update(delta);
            value = moduler.value - strength;
        }
    }
}
