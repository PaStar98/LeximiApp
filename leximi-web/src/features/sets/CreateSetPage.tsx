import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import { learningSetService, categoryService } from '../../api/services';
import { UpdateLearningSetDto, CreateLearningSetDto, UpdateLearningItemDto } from '../../types';
import SetEditor from './SetEditor';
import { useState } from 'react';

const CreateSetPage = () => {
    const navigate = useNavigate();
    const [categoryName, setCategoryName] = useState('');

    const { data: categories } = useQuery({
        queryKey: ['categories'],
        queryFn: categoryService.getAll
    });

    const createSetMutation = useMutation({
        mutationFn: (data: CreateLearningSetDto) => learningSetService.create(data),
        onSuccess: (newSet) => {
            navigate(`/sets/${newSet.id}`);
        }
    });

    const handleSubmit = async (data: UpdateLearningSetDto) => {
        if (!categoryName.trim()) {
            alert('Wpisz nazwę kategorii!');
            return;
        }

        try {
            let finalCategoryId = '';
            const existingCategory = categories?.find(
                c => c.name.toLowerCase() === categoryName.trim().toLowerCase()
            );

            if (existingCategory) {
                finalCategoryId = existingCategory.id;
            } else {
                const newCat = await categoryService.create({ name: categoryName.trim() });
                finalCategoryId = newCat.id;
            }

            const createDto: CreateLearningSetDto = {
                ...data,
                categoryId: finalCategoryId
            };
            createSetMutation.mutate(createDto);
        } catch (error) {
            alert('Wystąpił błąd podczas przygotowywania kategorii.');
        }
    };

    return (
        <div className="app-main">
            <h1>Stwórz nowy zestaw</h1>

            <div className="form-group">
                <label htmlFor="category-input">Kategoria</label>
                <input
                    id="category-input"
                    value={categoryName}
                    onChange={e => setCategoryName(e.target.value)}
                    className="input-field"
                    placeholder="Wpisz nazwę kategorii (np. Historia, Angielski...)"
                    required
                />
                <datalist id="category-options">
                    {categories?.map(cat => (
                        <option key={cat.id} value={cat.name} />
                    ))}
                </datalist>
            </div>

            <SetEditor
                onSubmit={handleSubmit}
                isSubmitting={createSetMutation.isPending}
            />
        </div>
    );
};

export default CreateSetPage;
