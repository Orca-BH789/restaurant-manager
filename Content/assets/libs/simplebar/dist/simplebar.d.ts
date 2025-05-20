export as namespace SimpleBar;
export = SimpleBar;

declare namespace SimpleBar {
    interface KnownClassNamesOptions {
        contentEl?: string;
        contentWrapper?: string;
        offset?: string;
        mask?: string;
        wrapper?: string;
        placeholder?: string;
        scrollbar?: string;
        track?: string;
        heightAutoObserverWrapperEl?: string;
        heightAutoObserverEl?: string;
        visible?: string;
        horizontal?: string;
        vertical?: string;
        hover?: string;
        dragging?: string;
    }

    type ClassNamesOptions = KnownClassNamesOptions & {
        [className: string]: string;
    };

    interface Options {
        autoHide?: boolean;
        classNames?: ClassNamesOptions;
        forceVisible?: boolean | 'x' | 'y';
        direction?: 'rtl' | 'ltr';
        timeout?: number;
        clickOnTrack?: boolean;
        scrollbarMinSize?: number;
        scrollbarMaxSize?: number;
    }
}

declare class SimpleBar {
    static removeObserver(): void;
    static instances: Pick<WeakMap<HTMLElement, SimpleBar>, 'get' | 'has'>;

    constructor(element: HTMLElement, options?: SimpleBar.Options);

    recalculate(): void;
    getScrollElement(): HTMLElement;
    getContentElement(): HTMLElement;
    unMount(): void;

    el: HTMLElement;
}

// Define the WeakMap type if it's not recognized by TypeScript
declare class WeakMap<K extends object, V> {
    constructor(entries?: ReadonlyArray<[K, V]>);
    get(key: K): V | undefined;
    set(key: K, value: V): this;
    has(key: K): boolean;
    delete(key: K): boolean;
}

// You may need to define the SimpleBar.Options type if it's not provided elsewhere
declare namespace SimpleBar {
    interface Options {
        // Define your options here
    }
}
